function openFileDialog() {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        input.onchange = (event) => {
            // Resolve with the selected file path or name
            resolve(event.target.files[0].name);
        };
        input.click();
    });
}

// Function to open a Folder Dialog
function openFolderDialog() {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        input.webkitdirectory = true; // Allow selecting folders (only works in certain browsers)
        input.onchange = (event) => {
            // Resolve with the selected folder path or name
            resolve(event.target.files[0].webkitRelativePath.split('/')[0]);
        };
        input.click();
    });
}