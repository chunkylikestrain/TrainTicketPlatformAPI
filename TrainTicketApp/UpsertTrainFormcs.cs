using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class UpsertTrainForm : Form
    {
        private readonly ITrainService _trainService;
        public Train TrainToEdit { get; set; }

        
     
        // DI‐enabled ctor
        public UpsertTrainForm(ITrainService trainService)
            
        {
            _trainService = trainService;
        }

        // … your Load event fills the form fields from TrainToEdit, 
        // and Save button uses _trainService.CreateOrUpdateAsync(TrainToEdit) …
    }
}

