using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Notifications;

namespace GP.Concealment.MessageHandlers {


    class ModMessageHandler : SEGarden.Commons.Conceal.ConcealMessageHandler {

        protected override void ConcealCancelled(long entityId) {
            Notification notice = new AlertNotification() {
                Text = "Conceal cancelled for entity " + entityId
            };
            notice.Raise();
        }

        protected override void ConcealQueued(long entityId) {
            Notification notice = new AlertNotification() {
                Text = "Conceal queued for entity " + entityId
            };
            notice.Raise();
        }

    }

}
