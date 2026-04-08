/**
 * File Interop Functions for Blazor
 * Provides file download functionality while respecting CSP
 */

// Make functions available globally for Blazor interop
window.fileInterop = {
    downloadFile: function(filename, base64Content, mimeType) {
        const bytes = base64ToUint8Array(base64Content);
        if (!bytes) {
            console.error('fileInterop.downloadFile: Failed to decode base64 content');
            return false;
        }
        
        const blob = new Blob([bytes], { type: mimeType });
        const url = URL.createObjectURL(blob);
        
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Clean up the object URL
        setTimeout(() => URL.revokeObjectURL(url), 100);
        return true;
    }
};

function base64ToUint8Array(base64) {
    try {
        const binaryString = atob(base64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes;
    } catch (error) {
        console.error('Failed to decode base64:', error);
        return null;
    }
}
