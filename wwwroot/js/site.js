// Funciones de ayuda para el carrito
window.MyGarageSale = {
    Cart: {
        // Obtener items del carrito desde sessionStorage
        getCartItems: function() {
            try {
                const cartJson = sessionStorage.getItem('my-garage-sale-cart');
                return cartJson ? JSON.parse(cartJson) : [];
            } catch (error) {
                console.error('Error parsing cart data:', error);
                return [];
            }
        },
        
        // Guardar items del carrito en sessionStorage
        saveCartItems: function(cartItems) {
            try {
                sessionStorage.setItem('my-garage-sale-cart', JSON.stringify(cartItems));
                return true;
            } catch (error) {
                console.error('Error saving cart data:', error);
                return false;
            }
        },
        
        // Limpiar carrito
        clearCart: function() {
            try {
                sessionStorage.removeItem('my-garage-sale-cart');
                return true;
            } catch (error) {
                console.error('Error clearing cart:', error);
                return false;
            }
        },
        
        // Verificar si sessionStorage está disponible
        isStorageAvailable: function() {
            try {
                const test = '__storage_test__';
                sessionStorage.setItem(test, test);
                sessionStorage.removeItem(test);
                return true;
            } catch (error) {
                return false;
            }
        }
    },
    
    // Función para scroll suave
    scrollToTop: function() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    
    // Función para copiar al portapapeles
    copyToClipboard: function(text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        } else {
            // Fallback para navegadores más antiguos
            const textArea = document.createElement("textarea");
            textArea.value = text;
            textArea.style.position = "fixed";
            textArea.style.left = "-999999px";
            textArea.style.top = "-999999px";
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            const result = document.execCommand('copy');
            textArea.remove();
            return result ? Promise.resolve() : Promise.reject();
        }
    }
};

// Inicialización cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    // Verificar compatibilidad con sessionStorage
    if (!MyGarageSale.Cart.isStorageAvailable()) {
        console.warn('SessionStorage no está disponible. El carrito puede no funcionar correctamente.');
    }
    
    // Agregar clases de animación para elementos que aparecen
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-fade-in');
            }
        });
    });
    
    // Observar elementos con la clase fade-in-on-scroll
    document.querySelectorAll('.fade-in-on-scroll').forEach(el => {
        observer.observe(el);
    });
});

// Función para mostrar notificaciones toast (si es necesario en el futuro)
window.showToast = function(message, type = 'info') {
    // Esta función se puede implementar más adelante si necesitas notificaciones
    console.log(`${type.toUpperCase()}: ${message}`);
}; 