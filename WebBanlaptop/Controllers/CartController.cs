
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanlaptop.Models;
using WebGrease;

namespace WebBanlaptop.Controllers
{
    public class CartController : Controller
    {
        private static readonly ILog log =
          log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: Cart
        public List<Cart> GetCart() // tạo list giỏ hàng hoặc lấy giỏ hàng
        {
            List<Cart> lstSPInCart = Session["Cart"] as List<Cart>;
            if (lstSPInCart == null)
            {
                lstSPInCart = new List<Cart>();
                Session["Cart"] = lstSPInCart;
            }
            return lstSPInCart;
        }
        private int tongSoLuong() // lấy tổng số lượng trong giỏ hàng
        {
            List<Cart> lstgh = Session["Cart"] as List<Cart>;
            if (lstgh == null)
            {
                return 0;
            }
            return lstgh.Sum(n => n.soluong);
        }

        private double tongTien() // lấy tổng tiền của giỏ hàng
        {
            double tongTien = 0;
            List<Cart> listProductInCart = Session["Cart"] as List<Cart>;
            if (listProductInCart != null)
            {
                tongTien = listProductInCart.Sum(n => n.tongtien);
            }
            return tongTien;
        }

        private void capNhatSL(CHITIETHD cthd) // cập nhật số lượng tồn của mỗi giày
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var shoes = db.CHITIETSP.Single(p => p.MASP == cthd.MASP && p.MAMAU == cthd.MAMAU);
            shoes.SOLUONGTON = shoes.SOLUONGTON - cthd.SOLUONG;
            db.SaveChanges();
        }

        public ActionResult Cart() // giỏ hàng
        {
            List<Cart> lstSPInCart = GetCart();
            if (lstSPInCart.Count == 0)
            {
                return RedirectToAction("enmtyCart", "Cart");
            }
            KHACHHANG kh = (KHACHHANG)Session["User"];
            ViewBag.tsl = tongSoLuong();
            ViewBag.tt = tongTien();
            if (kh != null)
            {
                ViewBag.tenkh = kh.TENKH;
            }
            
            return View(lstSPInCart);
        }
        public ActionResult enmtyCart() // giỏ hàng
        {
            
            return View();
        }
        public ActionResult _CartPartial() // partial giỏ hàng hiện số lượng
        {
            ViewBag.tsl = tongSoLuong();
            return PartialView();
        }
        [HttpPost]
        public ActionResult themGH(int? masp, string strURl) // thêm sản phẩm vào giỏ hàng
        {
            int? mamau = null;
            int sl = 1;
            List<Cart> listProductInCart = GetCart();
            if (Request.Form["maMau"] == null)
            {
                return Redirect(strURl);
            }
            mamau = Int32.Parse(Request.Form["maMau"].ToString());
            sl = Int32.Parse(Request.Form["soluong"].ToString());
            Cart product = listProductInCart.Find(n => n.masp == masp && n.mamau == mamau);
            if (product == null)
            {
                product = new Cart(masp, mamau,sl);
                listProductInCart.Add(product);
                return Redirect(strURl);
            }
            else
            {
                product.soluong+=sl;
                return Redirect(strURl);
            }
        }
        [HttpPost]
        public ActionResult capNhatSLSP( int? masp , int? mamau, FormCollection f)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            List<Cart> listProductInCart = GetCart();
            var sp = listProductInCart.SingleOrDefault(n => n.masp == masp && n.mamau == mamau);
            if(sp != null)
            {
                sp.soluong = int.Parse(f["soluong"].ToString());
            }

            return RedirectToAction("Cart");
        }
        public ActionResult xoa1SPInCart(int? masp, int? mamau) // xóa 1 món hàng ra khỏi giỏ hàng
        {
            List<Cart> listProductInCart = GetCart();
            Cart product = listProductInCart.SingleOrDefault(n => n.masp == masp && n.mamau == mamau);
            if (product != null)
            {
                listProductInCart.RemoveAll(n => n.masp == masp && n.mamau == mamau);
            }
            if (listProductInCart.Count == 0)
            {
                return RedirectToAction("enmtyCart", "Cart");
            }
            return RedirectToAction("Cart");
        }
        public ActionResult xoaGH()
        {
            List<Cart> listProductInCart = GetCart();
            listProductInCart.Clear();
            return RedirectToAction("enmtyCart", "Cart");
        }
        public static string RandomChar()
        {
            var chars = "0123456789";
            var stringChars = new char[4];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        [HttpPost]
        public ActionResult DatHang(string strURL, FormCollection f)
        {

            if (Session["User"] == null || Session["User"].ToString() == "")
                return RedirectToAction("Login", "User", new { @strURL = strURL }); // truyền url để lưu trang web quay về sau khi login
            if (Session["Cart"] == null)
                return RedirectToAction("enmtyCart", "Cart");
            string tenKh = f["cusName"].ToString();
            string soDT = f["cusPhone"].ToString();
            string diaChi = f["cusStreet"].ToString();
            string yeuCau = f["cusRequest"].ToString();
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            
            HOADON ddh = new HOADON();
            KHACHHANG kh = (KHACHHANG)Session["User"];
            List<Cart> gh = GetCart();
            var checkMHD = db.HOADON;
            ddh.MAHD = "DH-" + DateTime.Now.ToString("ddMMyy") + "-" + RandomChar();
            foreach(var item in checkMHD)
            {
                if (item.MAHD.Equals(ddh.MAHD))
                {
                    ddh.MAHD = "DH." + DateTime.Now.ToString("ddMMyy") + "." + RandomChar();
                }
            }
            ddh.NGAYLAP = DateTime.Now;
            ddh.NGAYGIAO = DateTime.Now.AddDays(10);
            ddh.TONGTIEN = (decimal)tongTien();
            ddh.TRANGTHAI = 0;
            ddh.MAKH = kh.MAKH;
            ddh.DIACHIGIAOHANG = diaChi;
            ddh.YEUCAUKHAC = yeuCau;
            ddh.TENKH = tenKh;
            ddh.SDTKH = soDT;
            db.HOADON.Add(ddh);
            db.SaveChanges();
            
            foreach (var item in gh)
            {
                CHITIETHD cthd = new CHITIETHD();
                cthd.MAHD = ddh.MAHD;
                cthd.MAMAU = item.mamau;
                cthd.MASP = item.masp;
                cthd.SOLUONG = item.soluong;
                cthd.DONGIA = (decimal?)item.dongia;
                capNhatSL(cthd);
                db.CHITIETHD.Add(cthd);
            }
            db.SaveChanges();

            Session["Cart"] = null;
            return RedirectToAction("XacNhanDatHang", "Cart",ddh);
        }
        public ActionResult XacNhanDatHang(HOADON ddh)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var dh = db.HOADON.FirstOrDefault(n => n.MAHD == ddh.MAHD);
            return View(dh);
        }



        #region thanh toán vnpay
        [HttpPost]
        public ActionResult Payment(int id, string strURL, FormCollection f)
        {
            if (Session["User"] == null || Session["User"].ToString() == "")
                return RedirectToAction("Login", "User", new { @strURL = strURL }); // truyền url để lưu trang web quay về sau khi login
            if (Session["Cart"] == null)
                return RedirectToAction("enmtyCart", "Cart");

            //string tenKh = f["cusName"].ToString();
            //string soDT = f["cusPhone"].ToString();
            //string diaChi = f["cusStreet"].ToString();
            //string yeuCau = f["cusRequest"].ToString();
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();

            List<Cart> lstgiohang = Session["Cart"] as List<Cart>;
            Cart gh = lstgiohang.Find(n => n.masp == id);
            SANPHAM sp = db.SANPHAM.Find(id);
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", (gh.tongtien * 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Ultil.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang : sản phẩm" + sp.TENSP); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }
        [HttpGet]
        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }
        #endregion


    }

}