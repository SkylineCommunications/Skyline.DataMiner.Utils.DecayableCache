# Skyline.DataMiner.Utils.DecayableCache.Protocol

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

## Ideal Use Cases
- Connectors where the parameters or tables are large and lookups need to be done frequently.
- Environments with concurrent data access needs.

## Getting Started

Define a static object of the **GlobalparameterCache** class using your desired data type.

- Tables could be a Dictionary holding the PK to some or all row values.
- Large parameters could contain the deserialized data type rather than the serialized string.
- Custom classes can be used to provide and abstract additional parameter specific functionality.

Defined as a *static*, it is immediately accessible to all referencing QActions, but is also shared amongst all elements hosted in the same SLScripting. Hence, elements not accessing their values will eventually be removed.

```CS
static GlobalParameterCache<Dictionary<PhysicalAddress, string>> sharedMacToPKCache = new GlobalParameterCache<Dictionary<PhysicalAddress, string>>(TimeSpan.FromMinutes(30));
``` 

The object retrieved from the cache will be a wrapper that contains an IsInitialized flag to be controlled by the user, but false by default, and a lock object to aid with safe access.

```CS
var parameter = sharedMacToPKCache.GetParameter(16, 32, 1000);
lock (parameter.Lock)
{
	if (!parameter.IsInitialized)
	{
		parameter.Value = new Dictionary<PhysicalAddress, string>();

		// Load existing values
	}

	// Read cached values
	// and/or write altered values to cache and element
}
``` 
