let createFormConfig = {
    patients: {
        title: "Patient",
        fields: [{ name: "firstName", label: "First Name", type: "text", required: true ,pattern: "^[\\p{L}\\s-]+$",minLength:3,maxLength:50 },
             { name: "lastName", label: "Last Name", type: "text", required: true ,pattern: "^[\\p{L}\\s-]+$",minLength:3,maxLength:50 },
             {name: "pesel", label: "PESEL", type: "text", required: true, pattern: "^[0-9]{11}$" },
              { name: "dateOfBirth", label: "Date of Birth", type: "date", required: true},
                { name: "phone", label: "Phone Number", type: "text", required: true, pattern: "^[+0-9\\s-]{9,15}$" },
               { name: "email", label: "Email", type: "email", minLength: 5, maxLength: 100 },
            ]
    },
    doctors: {
        title: "Doctor",}
};


const createBtn = document.getElementById('create-btn');
createBtn.addEventListener('click', handleCreate);
let currentEntity = document.getElementById('sql-entity').value.toLowerCase();



async function handleAdd(event) {
    event.preventDefault();  // KRYTYCZNE
    
    // Pobierz currentEntity z dropdownu (świeża wartość)
    const currentEntity = document.getElementById('sql-entity').value.toLowerCase();
    
    const config = createFormConfig[currentEntity];
    if (!config) {
        console.error("No config for entity:", currentEntity);
        return;
    }
    
    const submitBtn = event.target.querySelector('button[type=submit]');
    if (submitBtn) submitBtn.disabled = true;
    
    // Zbierz payload
    const payload = {};
    config.fields.forEach(field => {
        payload[field.name] = document.getElementById(field.name).value;
    });
    
    try {
        const response = await fetch(`https://localhost:7083/api/${currentEntity}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }
        
        alert(`${config.title} utworzony pomyślnie!`);
        closeModal();
    } catch (error) {
        console.error("Error creating item:", error);
        alert(`Błąd: ${error.message}`);
    } finally {
        if (submitBtn) submitBtn.disabled = false;
    }
}


async function handleCreate() {
    overlay.classList.remove('hidden');
    modal.classList.remove('hidden');
    modal.classList.add('center-table');1
    nextBtn.classList.add('hidden');
    prevBtn.classList.add('hidden');
    renderForm(currentEntity);
}
async function renderForm(entity) {
    const config = createFormConfig[entity];
    if (!config) {
        console.error("No config for entity:", entity);
        return;
    }
    const container = document.getElementById('result-container');
    let formHtml = `<h2>Create new ${config.title}</h2><form id="create-form">`;
    config.fields.forEach(field => {
        formHtml += `<div class="form-row">`;
                formHtml += `<label for="${field.name}">${field.label}:</label>`;
                formHtml += `<input type="${field.type}" class="form-input" id="${field.name}" name="${field.name}"
                ${field.required ? 'required' : ''} ${field.pattern ? `pattern="${field.pattern}"` : ''}
                ${field.minLength ? `minlength="${field.minLength}"` : ''}
                ${field.maxLength ? `maxlength="${field.maxLength}"` : ''}>`;
        formHtml += `</div>`;
    });
    formHtml += `<button type="submit"  id="add-btn" class="add-btn btn">Add</button></form>`;
    container.innerHTML = formHtml;
    document.getElementById('create-form').addEventListener('submit', handleAdd);
    
}