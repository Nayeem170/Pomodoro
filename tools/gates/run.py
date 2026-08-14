#!/usr/bin/env python3
"""Gate orchestrator - deterministic build+test gates with state files.

Runs format, build, test, and line-coverage gates matching the CI pipeline.
Writes per-gate JSON state files to .gates/. Halts on first failure.

Usage:
    python tools/gates/run.py
    python tools/gates/run.py --ticket T-001 --base develop
    python tools/gates/run.py --ticket T-001 --cleanup
    python tools/gates/run.py --workdir /path/to/worktree
    python tools/gates/run.py --skip-format
"""

import json
import re
import shutil
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent

COVERAGE_DIR = "coverage"
COVERAGE_THRESHOLD = 99.5

GATES = [
    (
        "gate2.format",
        ["dotnet", "format", "Pomodoro.sln", "--verify-no-changes"],
        "Verify code formatting matches dotnet format style",
        300,
    ),
    (
        "gate3.build",
        ["dotnet", "build", "Pomodoro.sln", "--configuration", "Release"],
        "Build solution in Release configuration",
        600,
    ),
    (
        "gate4.test",
        [
            "dotnet", "test", "Pomodoro.sln", "--configuration", "Release",
            "--verbosity", "normal",
            "--collect:XPlat Code Coverage", "--results-directory", "./coverage",
        ],
        "Run all unit tests in Release configuration with coverage collection",
        1200,
    ),
    (
        "gate5.coverage",
        None,
        "Check line coverage from cobertura report against 99.5% threshold",
        None,
    ),
]


def run_gate(name, cmd, description, workdir, output_dir, timeout=300):
    state = {
        "gate": name,
        "description": description,
        "command": " ".join(cmd),
        "status": "running",
        "startedAt": datetime.now(timezone.utc).isoformat(),
        "exitCode": None,
        "stdout": None,
        "stderr": None,
        "completedAt": None,
    }

    state_path = output_dir / f"{name}.json"
    state_path.write_text(json.dumps(state, indent=2))

    sep = "=" * 60
    print(f"\n{sep}")
    print(f"  {name}: {description}")
    print(f"{sep}")
    print(f"  $ {state['command']}\n")

    try:
        result = subprocess.run(
            cmd,
            cwd=str(workdir),
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except subprocess.TimeoutExpired:
        state["status"] = "timeout"
        state["completedAt"] = datetime.now(timezone.utc).isoformat()
        state_path.write_text(json.dumps(state, indent=2))
        print(f"  TIMEOUT after {timeout}s")
        return False

    state["exitCode"] = result.returncode
    state["stdout"] = result.stdout[-2000:] if result.stdout else None
    state["stderr"] = result.stderr[-2000:] if result.stderr else None
    state["completedAt"] = datetime.now(timezone.utc).isoformat()

    if result.returncode == 0:
        state["status"] = "passed"
        print(f"  PASSED (exit 0)")
    else:
        state["status"] = "failed"
        print(f"  FAILED (exit {result.returncode})")
        if result.stderr:
            for line in result.stderr.strip().splitlines()[-10:]:
                print(f"    | {line}")

    state_path.write_text(json.dumps(state, indent=2))
    return result.returncode == 0


def run_coverage_gate(name, description, workdir, output_dir):
    state = {
        "gate": name,
        "description": description,
        "command": f"parse {COVERAGE_DIR}/**/coverage.cobertura.xml (threshold {COVERAGE_THRESHOLD}%)",
        "status": "running",
        "startedAt": datetime.now(timezone.utc).isoformat(),
        "exitCode": None,
        "stdout": None,
        "stderr": None,
        "completedAt": None,
    }

    state_path = output_dir / f"{name}.json"
    state_path.write_text(json.dumps(state, indent=2))

    sep = "=" * 60
    print(f"\n{sep}")
    print(f"  {name}: {description}")
    print(f"{sep}")
    print(f"  $ {state['command']}\n")

    def finish(status, exit_code, summary, error=None):
        state["status"] = status
        state["exitCode"] = exit_code
        state["stdout"] = summary
        state["stderr"] = error
        state["completedAt"] = datetime.now(timezone.utc).isoformat()
        state_path.write_text(json.dumps(state, indent=2))
        print(f"  {summary}")
        if error:
            print(f"  FAILED: {error}")
            print(f"  {status.upper()} (exit {exit_code})")
        else:
            print(f"  PASSED (exit {exit_code})")
        return exit_code == 0

    coverage_root = workdir / COVERAGE_DIR
    reports = sorted(coverage_root.rglob("coverage.cobertura.xml")) if coverage_root.exists() else []

    if len(reports) == 0:
        return finish("failed", 1, f"No coverage report found under ./{COVERAGE_DIR}",
                      f"No coverage report found under ./{COVERAGE_DIR}")

    if len(reports) > 1:
        listing = "\n".join(f"    | {r}" for r in reports)
        return finish("failed", 1, f"Multiple coverage reports found:\n{listing}",
                      "Multiple coverage reports found (expected exactly one)")

    report_text = reports[0].read_text(encoding="utf-8", errors="replace")
    covered_match = re.search(r'lines-covered="(\d+)"', report_text)
    valid_match = re.search(r'lines-valid="(\d+)"', report_text)

    if not covered_match or not valid_match:
        return finish("failed", 1, "Could not parse line coverage from report",
                      "Could not parse lines-covered/lines-valid from cobertura report")

    covered = int(covered_match.group(1))
    valid = int(valid_match.group(1))
    if valid == 0:
        return finish("failed", 1, "Could not parse line coverage from report",
                      "lines-valid is 0 in cobertura report")

    percent = covered / valid * 100
    summary = f"Line coverage: {covered}/{valid} ({percent:.2f}%)"

    if percent < COVERAGE_THRESHOLD:
        return finish("failed", 1, summary,
                      f"Line coverage {percent:.2f}% is below threshold {COVERAGE_THRESHOLD}%")

    return finish("passed", 0, summary)


def worktree_base():
    return REPO_ROOT / ".." / ".worktrees-pomodoro"


def spawn(branch, base="develop"):
    base_path = Path(worktree_base()).resolve()
    base_path.mkdir(parents=True, exist_ok=True)

    slug = branch.replace("/", "-")
    wt_path = base_path / slug

    if wt_path.exists():
        print(f"  Worktree already exists at {wt_path}")
        return wt_path

    print(f"  Spawning worktree: {wt_path}")
    print(f"  Branch: {branch} from {base}")

    result = subprocess.run(
        ["git", "worktree", "add", "-b", branch, str(wt_path), base],
        cwd=str(REPO_ROOT),
        capture_output=True,
        text=True,
    )

    if result.returncode != 0:
        print(f"  FAILED to create worktree:")
        for line in result.stderr.strip().splitlines():
            print(f"    | {line}")
        sys.exit(1)

    print(f"  Worktree ready: {wt_path}")
    return wt_path


def teardown(branch, force=False):
    base_path = Path(worktree_base()).resolve()
    slug = branch.replace("/", "-")
    wt_path = base_path / slug

    if not wt_path.exists():
        print(f"  Worktree not found at {wt_path}, skipping teardown")
        return

    print(f"  Tearing down worktree: {wt_path}")

    args = ["git", "worktree", "remove"]
    if force:
        args.append("--force")
    args.append(str(wt_path))

    result = subprocess.run(
        args, cwd=str(REPO_ROOT), capture_output=True, text=True
    )

    if result.returncode != 0:
        print(f"  FAILED to remove worktree:")
        for line in result.stderr.strip().splitlines():
            print(f"    | {line}")
        return

    print(f"  Worktree removed")

    if force:
        subprocess.run(
            ["git", "branch", "-D", branch],
            cwd=str(REPO_ROOT),
            capture_output=True,
            text=True,
        )
        print(f"  Branch '{branch}' deleted")


def main():
    output_dir = REPO_ROOT / ".gates"
    output_dir.mkdir(exist_ok=True)

    args = sys.argv[1:]
    workdir = REPO_ROOT
    ticket = None
    base_branch = "develop"
    cleanup_only = False
    skip_format = False

    i = 0
    while i < len(args):
        if args[i] == "--workdir" and i + 1 < len(args):
            workdir = Path(args[i + 1]).resolve()
            i += 2
        elif args[i] == "--ticket" and i + 1 < len(args):
            ticket = args[i + 1]
            i += 2
        elif args[i] == "--base" and i + 1 < len(args):
            base_branch = args[i + 1]
            i += 2
        elif args[i] == "--cleanup":
            cleanup_only = True
            i += 1
        elif args[i] == "--skip-format":
            skip_format = True
            i += 1
        else:
            i += 1

    if ticket and cleanup_only:
        wt_branch = f"ticket/{ticket}"
        print(f"Cleaning up worktree for {ticket}")
        teardown(wt_branch, force=True)
        return

    wt_branch = None
    if ticket:
        wt_branch = f"ticket/{ticket}"
        header = "#" * 60
        print(f"\n{header}")
        print(f"  Ticket: {ticket}")
        print(f"  Branch: {wt_branch} (from {base_branch})")
        print(f"{header}")

        workdir = spawn(wt_branch, base_branch)
        output_dir = workdir / ".gates"
        output_dir.mkdir(exist_ok=True)

    gates = list(GATES)
    if skip_format:
        gates = [g for g in gates if g[0] != "gate2.format"]

    print(f"Pipeline: {len(gates)} gates | workdir: {workdir} | output: {output_dir}")

    passed = 0
    for gate in gates:
        name, cmd, desc = gate[0], gate[1], gate[2]
        timeout = gate[3] if len(gate) > 3 and gate[3] else 300

        if name == "gate4.test":
            stale = workdir / COVERAGE_DIR
            if stale.exists():
                shutil.rmtree(stale, ignore_errors=True)
                print(f"Cleared stale coverage results at {stale}")

        if cmd is None:
            ok = run_coverage_gate(name, desc, workdir, output_dir)
        else:
            ok = run_gate(name, cmd, desc, workdir, output_dir, timeout)

        if ok:
            passed += 1
        else:
            print(f"\nHALT: {name} failed.")
            if ticket:
                print(f"Tearing down worktree for {ticket} (gates failed)...")
                teardown(wt_branch, force=True)
            sys.exit(1)

    print(f"\nAll {passed} gates passed.")
    if ticket:
        print(f"Worktree kept at: {workdir}")
        print(f"Branch: {wt_branch}")
        print(f"To cleanup: python tools/gates/run.py --ticket {ticket} --cleanup")


if __name__ == "__main__":
    main()
