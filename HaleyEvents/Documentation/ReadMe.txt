1. Read about SynchronizationContext
References: 
https://hamidmosalla.com/2018/06/24/what-is-synchronizationcontext/
https://blog.stephencleary.com/2017/03/aspnetcore-synchronization-context.html
https://www.codeproject.com/Articles/5274751/Understanding-the-SynchronizationContext-in-NET-wi

KEY POINTS:
//Another important thing is that every Thread has its own SynchronizationContext. That means if we delegate work from one thread pool to another thread, we can get the snapshot of the current running environment and pass it to another thread.


