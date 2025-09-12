using System;

namespace Bidzy.Domain.Enties
{
    public class AppReview
    {
        public Guid Id { get; set; } 

        public Guid UserId { get; set; }          
                
        public string FullName { get; set; } 
        public int Rating { get; set; }    
        public string Comment { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
