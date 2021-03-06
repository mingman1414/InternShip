using AutoMapper;
using Falcon.Web.Core.Helpers;
using Phoenix.Server.Data.Entity;
using Phoenix.Server.Services.Database;
using Phoenix.Shared.Common;
using Phoenix.Shared.Core;
using Phoenix.Shared.ProductSKU;
using Phoenix.Shared.Rating;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix.Server.Services.MainServices
{
    public interface IRatingService
    {
        Rating GetRatingsById(int id);

        Task<BaseResponse<RatingDto>> GetAllRatings(RatingRequest request);

        Task<BaseResponse<RatingDto>> DeleteRatingsById(int Id);
        
        Task<BaseResponse<RatingAppDto>> GetRatingByProductSKUId(RatingAppRequest request);

        Task<CrudResult> AddRating(RatingAppRequest request);

        Task<BaseResponse<RatingAppDto>> GetRateAndSum(RatingAppRequest request);
        Task<CrudResult> UpdateRating(RatingAppRequest ratingAppRequest);
    }
    public class RatingService : IRatingService
    {
        private readonly DataContext _dataContext;
        public RatingService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Lấy danh sách đánh giá
        public async Task<BaseResponse<RatingDto>> GetAllRatings(RatingRequest request)
        {
            var result = new BaseResponse<RatingDto>();
            try
            {
                //setup query
                var query = _dataContext.Ratings.AsQueryable();

                
                //filter
                if (request.Rate > 0)
                {
                    query = query.Where(d => d.Rate == request.Rate);
                }

                if (!string.IsNullOrEmpty(request.Comment))
                {
                    query = query.Where(d => d.Comment.Contains(request.Comment));
                }

                if (request.Customer_Id > 0)
                {
                    //query = query.Where(d => d.Customer_Id == request.Customer_Id);
                    query = query.Where(d => d.Customer.FullName == request.Customer_Name);
                }

                if (request.ProductSKU_Id > 0)
                {
                    query = query.Where(d => d.ProductSKU_Id == request.ProductSKU_Id);
                }

                if (request.Deleted == false)
                {
                    query = query.Where(d => d.Deleted.Equals(request.Deleted));
                }

                query = query.OrderByDescending(d => d.Id);

                var data = await query.Skip(request.Page * request.PageSize).Take(request.PageSize).ToListAsync();
                result.DataCount = (int)((await query.CountAsync()) / request.PageSize) + 1;
                result.Data = data.MapTo<RatingDto>();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        // Get Rating By Id
        public Rating GetRatingsById(int id) => _dataContext.Ratings.Find(id);

        // Delete Rating
        public async Task<BaseResponse<RatingDto>> DeleteRatingsById(int Id)
        {
            var result = new BaseResponse<RatingDto>();
            try
            {

                var ratings = GetRatingsById(Id);
                
                ratings.Deleted = true;

                await _dataContext.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        #region GetRatingByProductSKUId
        public async Task<BaseResponse<RatingAppDto>> GetRatingByProductSKUId(RatingAppRequest request)
        {
            var result = new BaseResponse<RatingAppDto>();
            try
            {
                var query = (from r in _dataContext.Ratings
                             join c in _dataContext.Customers on r.Customer_Id equals c.Id
                             join s in _dataContext.ProductSKUs on r.ProductSKU_Id equals s.Id
                             join p in _dataContext.Products on s.Product_Id equals p.Id
                             join i in _dataContext.ImageRecords on p.Image1 equals i.Id
                             select new
                             {
                                 Id = r.Id,
                                 ProductName = p.Name,
                                 Rate = r.Rate,
                                 ProductImage = i.AbsolutePath,
                                 Ram = s.Ram,
                                 Storage = s.Storage,
                                 ModelCode = p.ModelCode,
                                 ProductTypeId = p.ProductType_Id,
                                 Comment = r.Comment,
                                 CreatedDate = r.CreatedDate,
                                 Image1 = r.Image1,
                                 Image2 = r.Image2,
                                 Image3 = r.Image3,
                                 Customer_Name = c.FullName,
                                 ProductSKU_Id = r.ProductSKU_Id,
                                 Order_Id = r.Order_Id
                             }).AsQueryable();
                if (request.ProductSKU_Id != 0)
                {
                    query = query.Where(d => d.ProductSKU_Id == request.ProductSKU_Id);
                }
                if (request.Order_Id != 0)
                {
                    query = query.Where(d => d.Order_Id == request.Order_Id);
                }

                var config = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
                var mapper = config.CreateMapper();
                var listcart = query.Select(mapper.Map<RatingAppDto>).ToList();
                result.Data = listcart.MapTo<RatingAppDto>();

            }
            catch (Exception ex)
            {

            }

            return result;
        }
        #endregion

        #region AddRating
        public async Task<CrudResult> AddRating(RatingAppRequest request)
        {
            var Rating = new Rating();
            Rating.Rate = request.Rate;
            Rating.Comment = request.Comment;
            Rating.CreatedDate = request.CreatedDate;
            Rating.Image1 = request.Image1;
            Rating.Image2 = request.Image2;
            Rating.Image3 = request.Image3;
            Rating.Customer_Id = request.Customer_Id;
            Rating.ProductSKU_Id = request.ProductSKU_Id;
            Rating.Order_Id = request.Order_Id;
            Rating.Deleted = request.Deleted;

            _dataContext.Ratings.Add(Rating);

            await _dataContext.SaveChangesAsync();
            //int a = Order.Id;
            return new CrudResult() { IsOk = true };
        }
        #endregion

        #region GetRatingByProductSKUId
        public async Task<BaseResponse<RatingAppDto>> GetRateAndSum(RatingAppRequest request)
        {
            var result = new BaseResponse<RatingAppDto>();
            try
            {
                var query = _dataContext.Ratings.AsQueryable();
                if (request.ProductSKU_Id != 0)
                {
                    query = query.Where(d => d.ProductSKU_Id == request.ProductSKU_Id);
                }

                int RateSum = query.Sum(r => r.Rate);
                int RateCount = query.Count();

                var data = await query.ToListAsync();
                result.DataCount = (int)((await query.CountAsync()) / request.PageSize) + 1;
                result.Data = data.MapTo<RatingAppDto>();

            }
            catch (Exception ex)
            {

            }

            return result;
        }
        #endregion

        #region UpdateCustomerDetail
        public async Task<CrudResult> UpdateRating(RatingAppRequest ratingAppRequest)
        {
            var query = _dataContext.Ratings.AsQueryable();
            if (ratingAppRequest.ProductSKU_Id != 0)
            {
                query = query.Where(d => d.ProductSKU_Id == ratingAppRequest.ProductSKU_Id);
            }

            int RateSum = query.Sum(r => r.Rate);
            int RateCount = query.Count();

            double Rating = Math.Round(((double)RateSum / RateCount), 2);

            int Id = ratingAppRequest.ProductSKU_Id;
            var ProductSKU = _dataContext.ProductSKUs.Find(Id);
            ProductSKU.Rating = Rating;
            
            await _dataContext.SaveChangesAsync();
            return new CrudResult() { IsOk = true };
        }
        #endregion
    }
}
