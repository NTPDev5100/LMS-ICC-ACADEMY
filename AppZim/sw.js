var linkurl = '';
self.addEventListener('push', function (event) {
    if (!(self.Notification && self.Notification.permission === 'granted')) {
        return;
    }

    var data = {};
    if (event.data) {
        data = event.data.json();
    }

    //console.log('Notification Recieved:');
    //console.log(data);

    var title = data.title;
    var message = data.message;
    var icon = data.icon;
    linkurl = data.link;

    event.waitUntil(self.registration.showNotification(title, {
        body: message,
        icon: icon,
        badge: icon
    }));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    //console.log(linkurl);
    if (linkurl !== '') {
        event.waitUntil(
            clients.openWindow(linkurl)
        );
    }
});