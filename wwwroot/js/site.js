// Arquivo JavaScript para funcionalidades adicionais
document.addEventListener('DOMContentLoaded', function () {
    // Inicializa tooltips do Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Adiciona confirmação para botões de remoção
    var removeButtons = document.querySelectorAll('.btn-danger');
    removeButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!confirm('Tem certeza que deseja remover este item?')) {
                e.preventDefault();
            }
        });
    });
});
