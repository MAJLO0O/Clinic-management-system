const syncButton = document.getElementById("sync-btn");
result = document.getElementById("seed-result");

syncButton.addEventListener("click",handleSync);

async function handleSync () {
    result.innerText= "Synchronizing...";
    syncButton.disabled = true;
     try{
        const response = await fetch("https://localhost:7083/api/admin/sync",{
            method: "POST",
            headers:{
                "Content-Type": "application/json"
            }
        });
        if (!response.ok) {
            throw new Error(`HTTP error: ${response.status}`);
        }
     result.innerText = "Db's sync'ed";
     }
     catch(error)
     {
        console.log(error);
        result.innerText = "Error while synchronizing data";
     }
     syncButton.disabled = false;
}
