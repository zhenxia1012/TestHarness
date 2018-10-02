# TestHarness

Remote Test Harness are mainly used for clients to automatically perform continuous integration and testing on the server side

Remote TestHarness consists of three components:

Client is the user of TestHarness. He could send test dll files to repository before sending test request to TestHarness server, and also he could query about the logs from repository. And client has an GUI interface to interact with this application.

TestHarness server can receive one or more test request from multiple clients and simultaneously handle many test requests by multiple threads. And each test request is handled in its own child thread and child Appdomian. As a result, TestHarness will send back the result back to client and repository.

Repository stores the Dlls files, log and result, responds to the query from client and sends Dlls files to TestHarness. 

All the communications between TestHarness, Client and Repository are realized by WCF and are asynchronous, which means Test libraries and Test Requests are sent to the Repository and Test Harness server, respectively, and results sent back to a requesting client, using an asynchronous message-passing communication channel. The Test Harness receives test libraries from the Repository using the same communication processing. 


