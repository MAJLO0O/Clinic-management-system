document.addEventListener('DOMContentLoaded', () => {
    const databaseOptions = document.getElementById("database-choice");
    const entities = document.getElementById("sql-entity");

    function updateVisibility() {
        if (databaseOptions.value === 'MongoDb') {
            entities.style.display = 'none';
        } else {
            entities.style.display = 'block';
        }
        
    }
    function updateEntityOptions() {
        currentEntity = document.getElementById('sql-entity').value.toLowerCase();
        const entitySelect = document.getElementById("sql-entity").value.toLowerCase();
        if (entitySelect === 'patients' || entitySelect === 'doctors') {
            document.getElementById('create-btn').style.display = 'block';
        } else {
            document.getElementById('create-btn').style.display = 'none';
        }
    }

    databaseOptions.addEventListener('change', updateVisibility);
    entities.addEventListener('change', updateEntityOptions);
    updateVisibility();
    updateEntityOptions();
});

