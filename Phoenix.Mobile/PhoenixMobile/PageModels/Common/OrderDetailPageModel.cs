using Phoenix.Mobile.Core.Infrastructure;
using Phoenix.Mobile.Core.Models.Order;
using Phoenix.Mobile.Core.Models.OrderDetail;
using Phoenix.Mobile.Core.Services;
using Phoenix.Mobile.Core.Services.Common;
using Phoenix.Mobile.Helpers;
using Phoenix.Shared.CartItem;
using Phoenix.Shared.Order;
using Phoenix.Shared.OrderDetail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Phoenix.Mobile.PageModels.Common
{
    public class OrderDetailPageModel : BasePageModel
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IDialogService _dialogService;
        private readonly ICartItemService _cartItemService;
        private readonly IWorkContext _workContext;

        public OrderDetailPageModel(IOrderService orderService, IDialogService dialogService, IOrderDetailService orderDetailService, ICartItemService cartItemService, IWorkContext workContext)
        {
            _orderService = orderService;
            _dialogService = dialogService;
            _orderDetailService = orderDetailService;
            _cartItemService = cartItemService;
            _workContext = workContext;
        }

        public OrderModel Order { get; set; }

        public override async void Init(object initData)
        {
            if (initData != null)
            {
                Order = (OrderModel)initData;
            }
            else
            {
                Order = new OrderModel();
            }
            NavigationPage.SetHasNavigationBar(CurrentPage, false);
            CurrentPage.Title = "Chi tiết đơn hàng";
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            await LoadData();
        }

        private async Task LoadData()
        {
            var token = _workContext.Token;
            UserId = token.UserId;
            request.Order_Id = Order.Id;
            var data = await _orderDetailService.GetOrderDetailHistory(request);
            if (data == null)
            {
                await _dialogService.AlertAsync("Lỗi kết nối mạng!", "Lỗi", "OK");
            }
            else
            {
                OrderDetails = data;
                ListViewHeight = 120 * OrderDetails.Count;
                RaisePropertyChanged(nameof(OrderDetails));
                if (Order.DeliveryDate == null)
                {
                    RatingButton = false;
                    ViewRatingButton = false;
                    if (Order.CancelRequest)
                    {
                        CancelButton = false;
                    }
                    else
                    {
                        // EnableButton = true;
                        CancelButton = true;
                    }
                }
                else
                {
                    if (!Order.IsRated)
                    {
                        RatingButton = true;
                        ViewRatingButton = false;
                        CancelButton = false;
                    }
                    else
                    {
                        RatingButton = false;
                        ViewRatingButton = true;
                        CancelButton = false;
                    }
                }
            }
        }

        #region properties
        public List<OrderDetailHistoryModel> OrderDetails { get; set; } = new List<OrderDetailHistoryModel>();
        public OrderDetailHistoryModel Details { get; set; } = new OrderDetailHistoryModel();
        public OrderDetailHistoryRequest request { get; set; } = new OrderDetailHistoryRequest();
        public bool RatingButton { get; set; }
        public bool ViewRatingButton { get; set; }
        public bool CancelButton { get; set; }
        public bool EnableButton { get; set; }
        public int ListViewHeight { get; set; }
        public int UserId { get; set; }

        #endregion

        #region ReBuyCommand
        public Command ReBuyCommand => new Command(async (p) => await ReBuyExecute(), (p) => !IsBusy);
        private async Task ReBuyExecute()
        {
            try
            {
                foreach (var item in OrderDetails)
                {
                    if (IsBusy) return;
                    IsBusy = true;

                    var data = await _cartItemService.AddItemToCart(new CartItemRequest
                    {
                        ProductSKU_Id = item.ProductSKU_Id,
                        Quantity = item.Quantity,
                        User_Id = UserId
                    });
                    await CoreMethods.PushPageModel<CartPageModel>();
                    //await _dialogService.AlertAsync("ProductSKU_Id: " + item.ProductSKU_Id + ", Quantity: " + item.Quantity + ", User_Id: ");
                    IsBusy = false;
                }
                

            }
            catch (Exception e)
            {
                await _dialogService.AlertAsync("Thêm thất bại");

            }
        }
        #endregion

        #region RatingCommand
        public Command RatingCommand => new Command(async (p) => await RatingExecute(), (p) => !IsBusy);
        private async Task RatingExecute()
        {
            //if (OrderDetails.Count == 1)
            //{
            //    request.Order_Id = Order.Id;
            //    var data = await _orderDetailService.GetOrderDetailHistoryById(request);
            //    Details = data;
            //    await CoreMethods.PushPageModel<RatingPageModel>(Details);
            //}
            //else
            //{
                await CoreMethods.PushPageModel<MultipleRatingPageModel>(OrderDetails);
            //}

        }
        #endregion

        #region ViewRatingCommand
        public Command ViewRatingCommand => new Command(async (p) => await ViewRatingExecute(), (p) => !IsBusy);
        private async Task ViewRatingExecute()
        {

            await CoreMethods.PushPageModel<ViewRatingPageModel>(Order);

        }
        #endregion
       
        #region CancelCommand
        public Command CancelCommand => new Command(async (p) => await CancelExecute(), (p) => !IsBusy);
        private async Task CancelExecute()
        {
            try
            {
                if (IsBusy) return;
                IsBusy = true;
                var data1 = await _orderService.EditOrder(Order.Id, new OrderAppRequest
                {
                    Id = Order.Id,
                    CancelRequest = true,
                    Status = "Yêu cầu hủy đang được xử lý"
                });
                IsBusy = false;
            }
            catch (Exception e)
            {
                await _dialogService.AlertAsync("Thêm Order Detail thất bại");

            }
            await CoreMethods.PopPageModel();
            await _dialogService.AlertAsync("Đã gửi yêu cầu hủy");

            IsBusy = false;
        }
        #endregion

    }
}
