window.modalHelper = {
    positionModal: function() {
        try {
            const modal = document.querySelector('.modal.show');
            if (!modal) return;
            
            const modalContent = modal.querySelector('.modal-content');
            if (!modalContent) return;
            
            // Reset any previous styles
            modalContent.style.position = 'relative';
            modalContent.style.top = 'auto';
            modalContent.style.height = 'auto';
            modalContent.style.maxHeight = '350px';
            modalContent.style.width = '450px';
            
            // Position the modal at the top of the viewport with some padding
            const viewportHeight = window.innerHeight;
            const modalHeight = Math.min(350, viewportHeight - 100);
            
            modalContent.style.marginTop = '80px';
            modalContent.style.marginBottom = '20px';
            
            // Ensure the footer is visible
            const footer = modalContent.querySelector('.modal-footer');
            if (footer) {
                footer.style.position = 'relative';
                footer.style.bottom = '0';
                footer.style.width = '100%';
            }
            
            console.log('Modal positioned by helper');
        } catch (e) {
            console.error('Error positioning modal:', e);
        }
    }
}; 