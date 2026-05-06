const testBtn = document.getElementById('test-btn');
testBtn.addEventListener('click', handleTest);

let values = [100,1000,5000,50000];
async function handleTest() {
    const results = [];
      testBtn.disabled = true;
            overlay.classList.remove('hidden');
            modal.classList.remove('hidden');
            nextBtn.classList.add('hidden');
            prevBtn.classList.add('hidden');
    try{
        for(let i=0;i<values.length;i++)
        {
          
        
            updateProgress(`Running ${i+1}/${values.length}: ${values[i]} records...`);
    let response = await fetch(`https://localhost:7083/api/admin/benchmark`,{
            method: "POST",
            headers:{
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ recordCount: values[i] })
        });
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    results.push(data);
        }
        renderTable(results);
        
    }
    catch(error){
        console.error('Error occurred:', error);
         document.getElementById('result-container').innerHTML = "<p>Error loading data</p>";}
         finally{
            testBtn.disabled = false;
         }
}
function updateProgress(message) {
    document.getElementById('result-container').innerHTML = `<p>${message}</p>`;
}