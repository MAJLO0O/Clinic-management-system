clearBtn = document.getElementById("clear-btn");
clearResult = document.getElementById("clear-result");


clearBtn.addEventListener("click",handleClear);

async function handleClear() {

    const confirmed = confirm("Are you sure? This will DELETE ALL DATA!");

    if (!confirmed) {
        return;
    }


    clearBtn.disabled = true;
    clearResult.innerText = "Clearing data...";

    try{
        const response = await fetch("https://localhost:7083/api/admin/clean", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            }
        });
        const result = await response.json();
        if(response.ok){
            clearResult.innerText = "Data cleared successfully!";
        }
    } catch (error) {
        clearResult.innerText = "Error clearing data.";
    } finally {
        clearBtn.disabled = false;
    }
}
