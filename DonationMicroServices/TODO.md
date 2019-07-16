# Donation-MicroService TODO list

trigger last notification in queue.processor 
-   if no new donation processed in the last 3 minutes, send current state notification

Implement waitForStatefulsets
kubectl get statefulsets -o json
kubectl get statefulsets queueprocessor-1-0-50-sfs -o json

Try url output error expected on failure

Remove action parameter from
await saNotificationPublisher.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationEnqueued, $"<!> final:{final}",

## Donation-QueueProcessor-Console
- queueprocessor-1-0-43-sfs-1 is logging donation info
- queueprocessor-1-0-43-sfs-1 is not logging donation info

## Donation-PersonSimulator-Console

- Handle better http post error [DONE]
    * HttpRequestException: Connection reset by peer
    * Retry once.

## Donation-RestApi-Entrance

- Handle start error
    * Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3] Failed to determine the https port for redirect.

## NOW

- Simulator should notify every 500 as info - [done]
- Queue processor should notify starting - [done]
https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings    

