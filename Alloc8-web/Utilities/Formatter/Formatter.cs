using Alloc8_web.ViewModels.User;

namespace Alloc8_web.Utilities.Formatter
{
    public class Formatter
    {
        public TimeFormatter time;
        public Formatter() 
        { 
            time = new TimeFormatter();
        }
        public void byUser(UserLoginDataViewModel? user)
        {
            if(user == null) 
            {
                return;
            }
            this.time.byTimeZone(user.timeZone);

        }
    }
}
