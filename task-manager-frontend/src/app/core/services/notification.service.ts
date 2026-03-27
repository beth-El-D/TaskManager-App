import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class NotificationService {

    showSuccess(message: string) {
        this.showNotification(message, 'success');
    }

    showError(message: string) {
        this.showNotification(message, 'error');
    }

    showInfo(message: string) {
        this.showNotification(message, 'info');
    }

    private showNotification(message: string, type: 'success' | 'error' | 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
      <div class="notification-content">
        <span class="notification-icon">${this.getIcon(type)}</span>
        <span class="notification-message">${message}</span>
        <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
      </div>
    `;

        // Add styles if not already added
        this.addStyles();

        // Add to DOM
        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);

        // Add slide-in animation
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);
    }

    private getIcon(type: string): string {
        switch (type) {
            case 'success': return '✅';
            case 'error': return '❌';
            case 'info': return 'ℹ️';
            default: return 'ℹ️';
        }
    }

    private addStyles() {
        if (document.getElementById('notification-styles')) return;

        const styles = document.createElement('style');
        styles.id = 'notification-styles';
        styles.textContent = `
      .notification {
        position: fixed;
        top: 20px;
        right: 20px;
        min-width: 300px;
        max-width: 500px;
        padding: 16px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease-in-out;
        font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
      }

      .notification.show {
        transform: translateX(0);
      }

      .notification-success {
        background: #f0fff4;
        border-left: 4px solid #48bb78;
        color: #2d3748;
      }

      .notification-error {
        background: #fed7d7;
        border-left: 4px solid #e53e3e;
        color: #2d3748;
      }

      .notification-info {
        background: #ebf8ff;
        border-left: 4px solid #3182ce;
        color: #2d3748;
      }

      .notification-content {
        display: flex;
        align-items: center;
        gap: 12px;
      }

      .notification-icon {
        font-size: 18px;
        flex-shrink: 0;
      }

      .notification-message {
        flex: 1;
        font-size: 14px;
        line-height: 1.4;
      }

      .notification-close {
        background: none;
        border: none;
        font-size: 18px;
        cursor: pointer;
        color: #718096;
        padding: 0;
        width: 24px;
        height: 24px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 4px;
        transition: background-color 0.2s;
      }

      .notification-close:hover {
        background-color: rgba(0, 0, 0, 0.1);
      }
    `;
        document.head.appendChild(styles);
    }
}