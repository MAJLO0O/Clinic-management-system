importBtn = document.getElementById("import-btn");
importResult = document.getElementById("export-result");

importBtn.addEventListener("click",handleImport);

async function handleImport() {

    importBtn.disabled = true;
    const fileInput = document.getElementById("import-file");
    const file = fileInput.files[0];
    
    
    if (!file) {
        importResult.innerText = "Please select a file to import.";
        importBtn.disabled = false;
        return;
    }
    if (!file.name.endsWith(".zip")) {
    importResult.innerText = "Only ZIP files are allowed";
    importBtn.disabled = false;
    return;
}
    try {
        const formData = new FormData();
        formData.append("file", file);
        const response = await fetch("https://localhost:7083/api/admin/import", {
            method: "POST",
            body: formData
        });
        const result = await response.json();
        if(response.ok){
            importResult.innerText = "Import successful!";
        }
    } catch (error) {
        importResult.innerText = "Error importing file.";
    } 
    finally {
        importBtn.disabled = false;
    }
}
