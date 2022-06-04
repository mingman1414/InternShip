using System;

namespace Phoenix.Shared.Rating
{
    public class RatingAppDto
    {
        public int Id { get; set; }
        public int Order_Id { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string Type
        {
            get
            {
                if (ProductTypeId == 5)
                    return "Loại: " + Ram + "/" + Storage;
                else
                    return "Model: " + ModelCode;
            }
        }
        public int ProductTypeId { get; set; }
        public string Ram { get; set; }
        public string Storage { get; set; }
        public string ModelCode { get; set; }
        public int Rate { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }

        public int Image1 { get; set; }

        public int Image2 { get; set; }

        public int Image3 { get; set; }

        public string Customer_Name { get; set; }
        public int ProductSKU_Id { get; set; }
        public bool Deleted { get; set; }
    }
}

