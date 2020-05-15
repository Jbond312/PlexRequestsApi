using Microsoft.Azure.WebJobs;

namespace PlexRequests.Functions.HttpTriggers
{
    public class StayWarmTrigger
    {
        [FunctionName("StayWarm")]
        // ReSharper disable once UnusedParameter.Global
        public static void StayWarm([TimerTrigger("*/15 * * * * *")]TimerInfo timer)
        {
            //Runs every 15 mins
        }
    }
}
