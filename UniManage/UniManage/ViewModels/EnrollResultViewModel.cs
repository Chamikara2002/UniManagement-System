namespace UniManage.ViewModels
{
    public class EnrollResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int EnrollmentId { get; set; }

        public static EnrollResultViewModel Success(int enrollmentId)
        {
            return new EnrollResultViewModel { Success = true, EnrollmentId = enrollmentId, Message = "Enrolled successfully." };
        }

        public static EnrollResultViewModel Failure(string message)
        {
            return new EnrollResultViewModel { Success = false, Message = message };
        }
    }
}