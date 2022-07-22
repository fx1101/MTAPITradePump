using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSTOrdersAPI;
using MtApi;
using MtApi.Monitors;

namespace MTAPITradePump
{
    //class NSTOrdMTAPI : NSTOrdersAPI.NSTOrders
    class NSTOrdMTAPI
    {
        private MtApiClient _apiClient;
        private TimerTradeMonitor _timerTradeMonitor;
        private TimeframeTradeMonitor _timeframeTradeMonitor;

        private const short MARKET_ORDER = 1;
        private const short STOP_ORDER = 2;
        private const short LIMIT_ORDER = 3;
        private const short STOPLIMIT_ORDER = 4;
        private const short MARKETCLOSE_ORDER = 5;

        private const short LONGENTRY_ACTION = 1;
        private const short SHORTENTRY_ACTION = 2;
        private const short LONGEXIT_ACTION = 3;
        private const short SHORTEXIT_ACTION = 4;

        public void initMTAPI()
        {
            this._apiClient = new MtApiClient();
            this._apiClient.BeginConnect(Properties.Settings.Default.MTAPIhost,
                                         Properties.Settings.Default.MTAPIport);
            _timerTradeMonitor = new TimerTradeMonitor(_apiClient) { Interval = 10000 }; // 10 sec
            _timerTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;

            _timeframeTradeMonitor = new TimeframeTradeMonitor(_apiClient);
            _timeframeTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;
        }

        private void _tradeMonitor_AvailabilityOrdersChanged(object sender, AvailabilityOrdersEventArgs e)
        {
            if (e.Opened != null)
            {
                //LogMessage($"{sender.GetType()}: Opened orders - {string.Join(", ", e.Opened.Select(o => o.Ticket).ToList())}");
            }

            if (e.Closed != null)
            {
                //LogMessage($"{sender.GetType()}: Closed orders - {string.Join(", ", e.Closed.Select(o => o.Ticket).ToList())}");
            }
        }

        private bool allowOrderModificaton;
        public bool AllowOrderModification
        {
            get { return this.allowOrderModificaton; }
            set { this.allowOrderModificaton = value; }
        }

        private bool brokerageFeedBack;
        public bool BrokerageFeedBack
        {
            get { return this.brokerageFeedBack; }
            set { this.brokerageFeedBack = value; }
        }

        private bool distinctShortOrders;
        public bool DistinctShortOrders
        {
            get { return this.distinctShortOrders; }
            set { this.distinctShortOrders = value; }
        }

        public int Message(string msgTxt)
        {
            var result = MessageBox.Show(msgTxt, "caption");
            return 0;
        }

        public int OrderCanceled(string OrderId)
        {
            if (_apiClient.OrderClose(Convert.ToInt32(OrderId), Properties.Settings.Default.MTAPIslip))
                return 0;
            return -1;
        }

        public int OrderFilled(string OrderId, int Shares, double FillPrice)
        {
            return 0;
        }

        public int OrderInactive(string OrderId)
        {
            return 0;
        }
        
    }
}
