let updateFormConfig = {
    patients: {
        title: "Patient",
        fields: [{ name: "firstName", label: "First Name", type: "text", required: true ,pattern: "^[\\p{L}\\s]+$",minLength:3,maxLength:50 },
             { name: "lastName", label: "Last Name", type: "text", required: true ,pattern: "^[\\p{L}\\s]+$",minLength:3,maxLength:50 },
                { name: "phone", label: "Phone Number", type: "text", required: true, pattern: "^[+0-9\\s]{9,15}$" },
               { name: "email", label: "Email", type: "email", minLength: 5, maxLength: 100 },
            ]
    },
    doctors: {
        title: "Doctor",
        fields:[{ name: "firstName", label: "First Name", type: "text", required: true ,pattern: "^[\\p{L}\\s]+$",minLength:3,maxLength:50 },
                { name: "lastName", label: "Last Name", type: "text", required: true ,pattern: "^[\\p{L}\\s]+$",minLength:3,maxLength:50 },
                { name: "phone", label: "Phone Number", type: "text", required: true, pattern: "^[+0-9\\s]{9,15}$" },
                { name: "email", label: "Email", type: "email", minLength: 5, maxLength: 100 },
                {name: "branchId", label: "Branch", type: "radio", required: true, optionsEndpoint: "/api/branches"},
                {name: "specializationIds", label: "Specializations", type: "checkboxes", required: true, maxSelected: 3, optionsEndpoint: "/api/specializations"}
            ]
    }
};

let ItemId = null;

async function renderUpdateForm(entity, itemData) {
    let specializationData;
    let branchData;
    if(entity==='doctors')
    {
        const specializationResponse = await fetch(`https://localhost:7083/api/specializations`);
        specializationData = await specializationResponse.json();
        const branchResponse = await fetch(`https://localhost:7083/api/branches`);
        branchData = await branchResponse.json();
    }
    const config = updateFormConfig[entity];
    if (!config) {
        console.error("No config for entity:", entity);
        return;
    }
    const container = document.getElementById('result-container');
    let formHtml = `<h2>Update ${config.title}</h2><form id="update-form">`;
    config.fields.forEach(field => {
        formHtml += `<div class="form-row">`;
                formHtml += `<label for="${field.name}">${field.label}:</label>`;

                if(field.type === 'radio')
                {
                    formHtml +=`<fieldset class="fieldset-branch"><legend>Wybierz jedną opcję</legend>`

                    const currentCityId = branchData.find(s => s.city === itemData.city)?.id;
                    
                    branchData.forEach(branch => { 
                        const checked = (branch.id === currentCityId) ? 'checked' : '';
                    formHtml+= `<label><input type="radio" name="branch-choice" value="${branch.id}" ${checked}>${branch.city}, ${branch.address}</label>`
                    });
                    formHtml += `</fieldset>`
                }
                else if(field.type === 'checkboxes')
                {
                    formHtml +=`<fieldset class="fieldest-specialization"><legend>Wybierz jedną opcję</legend>`

                    const currentSpecIds = (itemData.specializations || [])
                    .map(name => specializationData.find(s => s.name === name)?.id)
                    .filter(id => id !== undefined);

                    specializationData.forEach(specialization =>{
                        const checked = currentSpecIds.includes(specialization.id) ? 'checked' : '';
                    formHtml+= `<label><input type="checkbox" name="specialization-choice" value="${specialization.id}" ${checked}>${specialization.name}</label>`
                    });
                    formHtml += `</fieldset>`
                }
                else{
                formHtml += `<input type="${field.type}" class="form-input" id="${field.name}" name="${field.name}" value="${itemData[field.name] || ''}"
                ${field.required ? 'required' : ''} ${field.pattern ? `pattern="${field.pattern}"` : ''}
                ${field.minLength ? `minlength="${field.minLength}"` : ''}
                ${field.maxLength ? `maxlength="${field.maxLength}"` : ''}>`;
                }

        formHtml += `</div>`;
    });
    formHtml += `<button type="submit"  id="add-btn" class="add-btn btn">Update</button></form>`;
    container.innerHTML = formHtml;
    document.getElementById('update-form').addEventListener('submit', handleUpdate);
}
async function openUpdateForm(id, currentEntity, itemData) {
    modal.classList.add('center-table');
    nextBtn.classList.add('hidden');
    prevBtn.classList.add('hidden');
    ItemId = id;
    renderUpdateForm(currentEntity, itemData);
}
async function handleUpdate(event) {
    event.preventDefault();  
    const itemId = ItemId;
    const currentEntity = document.getElementById('sql-entity').value.toLowerCase();
    console.log(currentEntity);
    const config = updateFormConfig[currentEntity];
    if (!config) {
        console.error("No config for entity:", currentEntity);
        return;
    }
    
    const submitBtn = event.target.querySelector('button[type=submit]');
    if (submitBtn) submitBtn.disabled = true;
    
    
    const payload = {};
    config.fields.forEach(field => {
        if (field.type === 'radio') {
        const selected = document.querySelector('input[name="branch-choice"]:checked');
        payload[field.name] = selected ? parseInt(selected.value) : null;
    }
    else if (field.type === 'checkboxes') {
        const checked = document.querySelectorAll('input[name="specialization-choice"]:checked');
        payload[field.name] = Array.from(checked).map(cb => parseInt(cb.value));
    }
    else{
        payload[field.name] = document.getElementById(field.name).value;
    }
    });
    
    try {
        const response = await fetch(`https://localhost:7083/api/${currentEntity}/${itemId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }
        
        alert(`${config.title} updated!`);
        closeModal();
        await handleRead();
    } catch (error) {
        console.error("Error updating item:", error);
        alert(`Błąd: ${error.message}`);
    } finally {
        if (submitBtn) submitBtn.disabled = false;
    }
}
async function handleDelete(id,button) {
    if (!confirm('Czy na pewno chcesz usunąć ten rekord?')) {
        return;
    }
    button.disabled = true; 
    try {
        const response = await fetch(`https://localhost:7083/api/${currentEntity}/${id}`, {
            method: "DELETE"
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }
        
        alert(`Rekord usunięty pomyślnie!`);
        closeModal();
        await handleRead();  
    } catch (error) {
        console.error("Error deleting item:", error);
        alert(`Błąd: ${error.message}`);
    }
    finally {
        button.disabled = false;
    }
}