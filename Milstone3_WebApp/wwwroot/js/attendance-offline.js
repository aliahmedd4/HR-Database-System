// Offline Attendance Sync Functionality

(function() {
    'use strict';

    const OFFLINE_STORAGE_KEY = 'offline_attendance_logs';
    const SYNC_ENDPOINT = '/Attendance/SyncOfflineLogs';

    // Check if browser supports localStorage
    if (typeof(Storage) === "undefined") {
        console.warn('LocalStorage is not supported. Offline sync will not work.');
        return;
    }

    // Save attendance log to offline storage
    function saveOfflineLog(action, data) {
        try {
            let logs = getOfflineLogs();
            const logEntry = {
                id: Date.now(),
                action: action, // 'CheckIn' or 'CheckOut'
                data: data,
                timestamp: new Date().toISOString(),
                synced: false
            };
            logs.push(logEntry);
            localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(logs));
            console.log('Attendance log saved offline:', logEntry);
            return logEntry.id;
        } catch (e) {
            console.error('Error saving offline log:', e);
            return null;
        }
    }

    // Get all offline logs
    function getOfflineLogs() {
        try {
            const logs = localStorage.getItem(OFFLINE_STORAGE_KEY);
            return logs ? JSON.parse(logs) : [];
        } catch (e) {
            console.error('Error reading offline logs:', e);
            return [];
        }
    }

    // Mark log as synced
    function markLogAsSynced(logId) {
        try {
            let logs = getOfflineLogs();
            logs = logs.map(log => {
                if (log.id === logId) {
                    log.synced = true;
                }
                return log;
            });
            localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(logs));
        } catch (e) {
            console.error('Error marking log as synced:', e);
        }
    }

    // Remove synced logs
    function removeSyncedLogs() {
        try {
            let logs = getOfflineLogs();
            logs = logs.filter(log => !log.synced);
            localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(logs));
        } catch (e) {
            console.error('Error removing synced logs:', e);
        }
    }

    // Sync offline logs to server
    async function syncOfflineLogs() {
        const logs = getOfflineLogs().filter(log => !log.synced);
        
        if (logs.length === 0) {
            console.log('No offline logs to sync');
            return { success: true, synced: 0 };
        }

        console.log(`Syncing ${logs.length} offline attendance logs...`);

        try {
            const response = await fetch(SYNC_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin', // Include cookies for authentication
                body: JSON.stringify({ logs: logs })
            });

            if (response.ok) {
                const result = await response.json();
                
                // Mark synced logs
                result.syncedIds.forEach(id => markLogAsSynced(id));
                
                // Remove synced logs
                removeSyncedLogs();
                
                console.log(`Successfully synced ${result.syncedIds.length} logs`);
                return { success: true, synced: result.syncedIds.length };
            } else {
                console.error('Failed to sync logs:', response.statusText);
                return { success: false, error: response.statusText };
            }
        } catch (error) {
            console.error('Error syncing offline logs:', error);
            return { success: false, error: error.message };
        }
    }

    // Get anti-forgery token
    function getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }

    // Check online/offline status
    function isOnline() {
        return navigator.onLine;
    }

    // Handle online event - sync when connection is restored
    window.addEventListener('online', function() {
        console.log('Connection restored. Syncing offline logs...');
        syncOfflineLogs().then(result => {
            if (result.success && result.synced > 0) {
                showNotification(`Successfully synced ${result.synced} attendance record(s)`, 'success');
                // Reload page to show updated attendance
                setTimeout(() => {
                    if (window.location.pathname.includes('/Attendance')) {
                        window.location.reload();
                    }
                }, 1000);
            }
        });
    });

    // Handle offline event
    window.addEventListener('offline', function() {
        console.log('Connection lost. Attendance will be saved offline.');
        showNotification('You are offline. Attendance will be synced when connection is restored.', 'warning');
    });

    // Show notification
    function showNotification(message, type) {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.remove();
        }, 5000);
    }

    // Enhanced attendance recording with offline support
    function recordAttendanceOffline(action, loginMethod, logoutMethod) {
        if (isOnline()) {
            // If online, proceed normally (form will submit)
            return true;
        } else {
            // If offline, save to localStorage
            const data = {
                action: action,
                loginMethod: loginMethod || 'Web',
                logoutMethod: logoutMethod || 'Web',
                date: new Date().toISOString()
            };
            
            const logId = saveOfflineLog(action, data);
            if (logId) {
                showNotification('Attendance recorded offline. It will be synced when connection is restored.', 'info');
                // Prevent form submission
                return false;
            } else {
                showNotification('Failed to save attendance offline.', 'danger');
                return false;
            }
        }
    }

    // Expose functions globally
    window.AttendanceOffline = {
        saveLog: saveOfflineLog,
        getLogs: getOfflineLogs,
        sync: syncOfflineLogs,
        isOnline: isOnline,
        record: recordAttendanceOffline
    };

    // Auto-sync on page load if online
    if (isOnline()) {
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                syncOfflineLogs();
            }, 1000);
        });
    }

    // Periodic sync check (every 30 seconds)
    setInterval(() => {
        if (isOnline()) {
            syncOfflineLogs();
        }
    }, 30000);

})();

