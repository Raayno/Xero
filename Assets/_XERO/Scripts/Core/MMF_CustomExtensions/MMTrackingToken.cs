namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A simple class used to track the completion of a feedback
    /// </summary>
    public class MMTrackingToken
    {
        public bool IsComplete { get; private set; }

        public void Complete()
        {
            IsComplete = true;
        }

        public void Cancel()
        {
            IsComplete = true;
        }
    }
}
