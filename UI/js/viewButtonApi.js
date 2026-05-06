const viewBtn = document.getElementById("view-btn");
const db = document.getElementById('database-choice');
const entity = document.getElementById('sql-entity');
const nextBtn = document.getElementById('next-btn');
const prevBtn = document.getElementById('prev-btn');
const overlay = document.querySelector('.overlay');
const modal = document.querySelector('.modal');
const closeModalBtn = document.getElementById('close-modal-btn');

db.addEventListener("change",reset);
entity.addEventListener("change",reset);

viewBtn.addEventListener("click",handleRead);
nextBtn.addEventListener("click",nextPage);
prevBtn.addEventListener("click",prevPage);
closeModalBtn.addEventListener("click",closeModal);
let cursor = 0;
let cursorStack = [];
let currentData = null;

async function handleRead(){
const dbValue = db.value;
const entityValue = entity.value.toLowerCase();
let url;
        if (dbValue === "MongoDb") {
        url = `https://localhost:7083/api/admin/mongoDb/appointments?lastId=${cursor}&pageSize=25`;
    } else {
        url = `https://localhost:7083/api/admin/postgreSql/${entityValue}?lastId=${cursor}&pageSize=25`;
    }
    try{
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    currentData = data;

    nextBtn.disabled = !data.hasNext;
    prevBtn.disabled = cursorStack.length === 0;

    renderTable(data.items);
     overlay.classList.remove('hidden');
    modal.classList.remove('hidden');

} catch (error) {
    console.error("Error fetching data:", error);
     document.getElementById('result-container').innerHTML = "<p>Error loading data</p>";
}
}

function renderTable(items){
    const container = document.getElementById('result-container');

    if(!items || items.length===0)
    {
        container.innerHTML = "<p>No data</p>";
        return;
    }
    let table = "<table class='read-table'><tr>"
    
    const keys = Object.keys(items[0]);
    keys.forEach(k=>{
        table += `<th>${k}</th>`;
    });
    table += "</tr>"
    items.forEach(item => {
        table += "<tr>";
        keys.forEach(key=>{
            table+=`<td>${item[key]}</td>`;
        });

        table += `<td><button class='btn update-btn' data-id="${item.id}">Update</button></td>`;
        table += `<td><button class='btn delete-btn' data-id="${item.id}">Delete</button></td>`;
        table += "</tr>";
    });
    table += "</table>"

    container.innerHTML = table;
}

function reset(){
    cursor = 0;
    currentData = null;
    cursorStack = [];
    document.getElementById('result-container').innerHTML = "";
        nextBtn.disabled = true;
        prevBtn.disabled = true;
}

function closeModal(){
    overlay.classList.add('hidden');
    modal.classList.add('hidden');
    modal.classList.remove('center-table');
    testBtn.disabled = false;
    reset();
}

function nextPage(){
    if(currentData && currentData.hasNext){
        cursorStack.push(cursor);
        cursor = currentData.nextCursor;
        handleRead();
    }
}

function prevPage(){
    if(cursorStack.length > 0){
        cursor = cursorStack.pop();
        handleRead();
    }
}

document.getElementById('result-container').addEventListener('click', (e) => {
    if (e.target.classList.contains('update-btn')) {
        const currentEntity = document.getElementById('sql-entity').value.toLowerCase();
        const id = parseInt(e.target.dataset.id);
        const itemData = currentData.items.find(item => item.id === id);
        if (!itemData) {
            console.error("Item data not found for id:", id);
            return;
        }
        openUpdateForm(id, currentEntity, itemData);
    }
    if (e.target.classList.contains('delete-btn')) {
        const id = parseInt(e.target.dataset.id);
        handleDelete(id,e.target);
    }
});
