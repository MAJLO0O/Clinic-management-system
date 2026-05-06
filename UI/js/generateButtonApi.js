const seedBtn = document.getElementById("seed-btn");
const recordCount = document.getElementById("recordCount");
seedResult = document.getElementById("seed-result");

seedBtn.addEventListener("click",handleSeed);


async function handleSeed() {
    seedBtn.disabled = true;
    seedResult.innerText = "";
const count = parseInt(recordCount.value);
if(!count || count<=0)
{
    seedResult.innerText = "Invalid number of records";
        return;
}
    seedResult.innerText = "Processing...";
console.log(count);
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

        seedResult.innerText = `Generated: ${data.processedCount}`;
    }
    catch(error){
        console.error(error)
        seedResult.innerText = "Error while seeding Data";
    }
    seedBtn.disabled = false;
}
