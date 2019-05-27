## EventDrive an Event Driven Application Engine
The idea is basically to have one way to work with events or messages in your applications that allows you to scale your app as you go through different phases of development. Event Driven Design gives you a lot of flexibility, but what is out there right now is pretty much roll your own every time because there are so many design considerations for each project, depending on what your doing you might want to only store your events on disk and some events might need to be really fast and you don't want to save them right now, but maybe in the future you change your mind and want to start saving those events.

This project can be used to make an application that is more traditional application but utilizing a message bus to communicate with microservices or other applications. It can also be used for building Event Sourced systems. Event Sourcing is a totally different way of writing applications than most people are used to. The events in the system make up the state. In otherwords you don't store data in a database or on disk and update it as you need to change it. Every event that lead up to the current data is available. You may only be interested in the "current state" but if you ever want to know how something came to be, you also have all the events that happened before that put it into the current state.

Using an event hub or message broker does not mean your application is event sourced. Event Sourced means your state is determined by the events or messsages, which means you need to have a message broker or event hub either internally in your application and/or in a seperate service. I see an Event Hub and a Message Broker as two different things, while I think they are used elsewhere interchangably as they are very similar. I define a message broker as something that ensures messages are delivered and handled in the way that you intend them to be, retrying when necessary, splitting up messages for blue/green and a/b testing, essentially doing a lot of dirty work to ensure messages are routed to an endpoint. An event hub is a simpler thing that just takes in events and spits them out.

Now that we know where I'm coming from with Event Hub and Message Brokers, lets talk about building applications with both of these ideas. In any application there are usually things better suited for an event hub and some things that require a message broker. The goal of this project is to make working with events/messages the same and allowing you to choose which messages require a hub or a broker. And also all the other decisions you might need to make about where to store the events, do they need to be encrypted, what transport to use, should all be abstracted away from how you write your application.

Message Hub == Event Hub
Message Broker != Message Hub

## Design Goals
1. Simple -- because complexity is the root of all evil
2. Security First -- Secure by default
3. Modular -- Mix and match to roll your own
4. Scalable -- From internal in memory events to distributed and stored on disk

## How to use this project
Eventually it will be a NuGet package and you will add NuGet packages for different implementations. I think the best way to setup different implementations for different messages is to create event groups with names and you can define how those message are handled by default. We're still in early stages but I'd like to have it look something like this in your Startup.cs

services.UseEventDrive().UseEventStore("MyAppEventGroup", connectionString).UseInMemoryStream("MyAppInMemory").UseSignalR("ExternalAppGroup", signalRHub);



## The state of this project
This is very much in the research stage. Nothing is finalized yet so don't depend on the interfaces. There has been no attempts to optimize anything yet. I'd like to get a few working samples and a few different event hub and message broker implementations before we start to finalize the interfaces. I'd like to shoot for these initial server/services:
EventStore
RabbitMQ
SignalR - for direct to application/server message passing
InMemoryStream
InMemoryQueue
OnDiskStream
OnDiskQueue

Future implementations:
Azure EventHub
Apache Kafka
WebAPI - direct to app messaging
UDP - Direct to IP and Broadcast
Postgresql
ZeroMQ
Any the community wants to build

I'm open to different names for this project.

## Testing Setup
dotnet user-secrets set "EventDrive:EventStoreConnectionString" "tcp://<Your connection string>"
