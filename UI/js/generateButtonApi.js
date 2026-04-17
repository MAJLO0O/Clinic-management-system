const seedBtn = document.getElementById("seed-btn");
const recordCount = document.getElementById("recordCount");
result = document.getElementById("seed-result");

seedBtn.addEventListener("click",handleSeed);


async function handleSeed() {
    seedBtn.disabled = true;
const count = parseInt(recordCount.value);

if(!count || count<=0)
{
    result.innerText = "Invalid number of records";
        return;
}
    result.innerText = "Processing...";

    try{

        const response = await fetch("https://localhost:7083/api/admin/generator", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ count: count })
        });
        if (!response.ok) {
            throw new Error(`HTTP error: ${response.status}`);
        }

        const data = await response.json();

        result.innerText = `Generated: ${data.processedCount}`;
    }
    catch(error){
        console.error(error)
        result.innerText = "Error while seeding Data";
    }
    seedBtn.disabled = false;
}
