window.compressionInterop = {
    gzipCompress: function(str) {
        return new Promise((resolve, reject) => {
            try {
                const encoder = new TextEncoder();
                const data = encoder.encode(str);
                const blob = new Blob([data]);

                const cs = new CompressionStream('gzip');
                const compressedStream = blob.stream().pipeThrough(cs);

                const reader = compressedStream.getReader();
                const chunks = [];

                function pump() {
                    return reader.read().then(({ done, value }) => {
                        if (done) {
                            const compressed = new Uint8Array(chunks.reduce((acc, chunk) => acc + chunk.length, 0));
                            let offset = 0;
                            for (const chunk of chunks) {
                                compressed.set(chunk, offset);
                                offset += chunk.length;
                            }
                            const base64 = btoa(String.fromCharCode.apply(null, compressed));
                            resolve(base64);
                            return;
                        }
                        chunks.push(value);
                        return pump();
                    });
                }

                pump().catch(reject);
            } catch (error) {
                reject(error);
            }
        });
    },

    gzipDecompress: function(base64) {
        return new Promise((resolve, reject) => {
            try {
                const binaryString = atob(base64);
                const bytes = new Uint8Array(binaryString.length);
                for (let i = 0; i < binaryString.length; i++) {
                    bytes[i] = binaryString.charCodeAt(i);
                }

                const blob = new Blob([bytes]);
                const ds = new DecompressionStream('gzip');
                const decompressedStream = blob.stream().pipeThrough(ds);

                const reader = decompressedStream.getReader();
                const chunks = [];

                function pump() {
                    return reader.read().then(({ done, value }) => {
                        if (done) {
                            const decompressed = new Uint8Array(chunks.reduce((acc, chunk) => acc + chunk.length, 0));
                            let offset = 0;
                            for (const chunk of chunks) {
                                decompressed.set(chunk, offset);
                                offset += chunk.length;
                            }
                            const decoder = new TextDecoder();
                            resolve(decoder.decode(decompressed));
                            return;
                        }
                        chunks.push(value);
                        return pump();
                    });
                }

                pump().catch(reject);
            } catch (error) {
                reject(error);
            }
        });
    }
};
