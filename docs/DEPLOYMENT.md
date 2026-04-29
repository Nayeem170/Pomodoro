# Deployment

## Overview

This project uses **GitHub Actions** for CI/CD and **Cloudflare Pages** for hosting.

## Live Demo

- **Production:** https://pomodoro.bitops.bd (auto-deploys from `main`)
- **Preview:** https://pomodoro-6un.pages.dev (auto-deploys from `develop`)

## Pipelines

### Production (`main`)

| Workflow | File | Steps |
|----------|------|-------|
| Deploy Production | `.github/workflows/deploy.yml` | Build → Unit Tests → Deploy to Cloudflare Pages |
| Test Reports | `.github/workflows/deploy-reports.yml` | Build → Unit Tests → E2E Tests → Upload coverage & e2e reports to GitHub Pages |

### Development (`develop`)

| Workflow | File | Steps |
|----------|------|-------|
| Dev Deploy | `.github/workflows/deploy-dev.yml` | Build → Unit Tests → Deploy to Cloudflare Pages (preview) |
| Dev E2E | `.github/workflows/deploy-dev-e2e.yml` | Build → Unit Tests → E2E Tests → Upload reports |

## Infrastructure

| Component | Service |
|-----------|---------|
| Hosting | Cloudflare Pages (free) |
| DNS | Cloudflare (`bitops.bd`) |
| CI/CD | GitHub Actions |
| Domain | `promodoro.bitops.bd` |

## Required Secrets (GitHub)

| Secret | Description |
|--------|-------------|
| `CLOUDFLARE_API_TOKEN` | Cloudflare API token with **Cloudflare Pages: Edit** permission |
| `CLOUDFLARE_ACCOUNT_ID` | Cloudflare account ID |
| `CODECOV_TOKEN` | Codecov upload token for coverage reports |

## Cloudflare Pages Config

- **Project name:** `pomodoro`
- **Build output:** `publish/wwwroot` (from `dotnet publish`)
- **SPA routing:** `wwwroot/_redirects` — `/* /index.html 200`
- **Cache headers:** `wwwroot/_headers` — immutable caching for `_framework/*`, no-cache for `index.html`
