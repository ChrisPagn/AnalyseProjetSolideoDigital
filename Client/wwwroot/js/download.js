window.analyseProjetDownload = {
    telechargerFichier: function (nomFichier, octets) {
        const blob = new Blob([new Uint8Array(octets)], { type: "application/zip" });
        const url = URL.createObjectURL(blob);
        const lien = document.createElement("a");
        lien.href = url;
        lien.download = nomFichier;
        document.body.appendChild(lien);
        lien.click();
        document.body.removeChild(lien);
        URL.revokeObjectURL(url);
    }
};
