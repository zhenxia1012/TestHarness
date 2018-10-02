# TestHarness

## Description
Remote Test Harness are mainly used for clients to automatically perform continuous integration and testing on the server side

Remote TestHarness consists of three components:

Client is the user of TestHarness. He could send test dll files to repository before sending test request to TestHarness server, and also he could query about the logs from repository. And client has an GUI interface to interact with this application.

TestHarness server can receive one or more test request from multiple clients and simultaneously handle many test requests by multiple threads. And each test request is handled in its own child thread and child Appdomian. As a result, TestHarness will send back the result back to client and repository.

Repository stores the Dlls files, log and result, responds to the query from client and sends Dlls files to TestHarness. 

All the communications between TestHarness, Client and Repository are realized by WCF and are asynchronous, which means Test libraries and Test Requests are sent to the Repository and Test Harness server, respectively, and results sent back to a requesting client, using an asynchronous message-passing communication channel. The Test Harness receives test libraries from the Repository using the same communication processing. 

## Main Jobs
* Developed GUI interface based on WPF for user program which supported to send file to repository and test request to Test Harness, query log from repository describing test result and using a key that combined the test developer identity and the current time.
* Supported handling multiple test requests simultaneously for Test Harness and for every request, established one AppDomain to run tests defined by test request with XML body
* Designed a repository to save test file from Client and test result from Test Harness and send test file to Test Harness when it ran test and log to client whenever he queried log
* Provided a message-passing communication system, based on WCF using either synchronous request/response or asynchronous one-way messaging
* Implemented “Message” with head and content. Supported to deliver different types of content, such as test file, test result, test request and so on, through XML and making them serializable


