namespace Bidzy.API.DTOs.adminDtos
{
    public class UserStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int  NewThisMonth { get; set; }
    }
}
