# Skyline.DataMiner.Utils.DecayableCache.Common

## About



### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exist. In addition, you can leverage DataMiner Development Packages to build you own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

## Overview
This library offers a robust solution for efficiently caching parameter data in memory. Designed specifically for .NET applications, it ensures thread-safe operations, allowing for concurrent data access without compromising data integrity or performance.

## Getting Started

The cache is based on a concurrent dictionary, so it can be used in a very similar fashion. First, we define the type and the stale time:
```CS
DecayableCache<Guid, MyUser> myTemporaryUserCache = new DecayableCache<Guid, MyUser>(TimeSpan.FromMinutes(10));
``` 

We can now insert and get our objects. Those that are retrieved will stay fresh, those that are not become stale and get removed automatically:
```CS
// Fetching a user by Guid from the cache or the slower database and make sure we return the value that is in the cache.
public MyUser GetUser(Guid userId)
{
    MyUser cachedUser;
    MyUser freshUser;
    
    while (!myTemporaryUserCache.TryGetValue(userId, out cachedUser))
    {
        if (freshUser == null)
        {
            freshUser = MyDatabase.LoadUser(userId);
        }        
    
        if (_cachesPerElement.TryAdd(key, freshUser))
        {
            return newValue;
        }
    }
    
    return cachedUser;
}
```
