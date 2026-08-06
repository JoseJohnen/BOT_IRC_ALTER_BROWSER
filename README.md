<h1>BOT IRC ALTER BROWSER</h1><h3>(Alpha)</h3>

An IRC bot created for allowing the browsing of Gopherspace and Geminispace from IRC as a proof of concept for the idea of "middleware browser", which works providing content through the idea of "client agnosticism" which dictates than the middleware should be the one serving the content of the server(s) to a client or clients which characteristics we know nothing about or we have minimal information about it, and than can vary widely between themselfs.

<h2>Available Commands:</h2>

<h4>GOPHER>gopher://somegopherurl.net</h4>

It makes the bot answer to you using the same channel you use to ask him (or in private if you whisper him the requeriment) the hole you wanted to browse.

<h4>GEMINI>gopher://somegeminiurl.net</h4>

It makes the bot answer to you using the same channel you use to ask him (or in private if you whisper him the requeriment) the gem you wanted to browse.

<h4>[B|b]</h4>

Once you are in a site, you can go back to the last site using [B] or [b] the bot will comunicate this to you after you browse at least one site.

<h4>[#]</h4> (being # a number)

Allows you to navigate through a link in the presented hole/gem.

<h2>Configurations (appsettings.json)</h2>

Using the relevant part of the file as an example

  "Gemini":<br>
    "StartHosting": "Allow", //Any other word will make the bot to NOT host a gemini gem.:<br>
    "RootFolder": "geminiRoot", //The name of the folder where the gem should be, you can change it if you want, the location however should be the same folder where you can find the application.:<br>
    "Port": "1965", //The port the service will use to deploy the gem.:<br>
    "Cert1": "cert.pem", //The place where the cert should be starting from the location of the application.:<br>
    "Cert2": "key.pem" //The place where the cert should be starting from the location of the application.:<br>
  <br>
  "Gopher":<br>
    "StartHosting": "Allow", //Any other word will make the bot to NOT host the gopher hole.:<br>
    "RootFolder": "gopherRoot", //The name of the folder where the hole should be, you can change it if you want, the location however should be the same folder where you can find the application.:<br>
    "Port": "7070", //The port the service will use to deploy the hole.:<br>
    "URL": "localhost" //the URL you are using for the hole, its required in order to allow correct internal navigation.:<br>
  <br>
  "Bots": //Notice than this is an array, in other words you can have several bots in several channels and several networks at the same time.<br>
      "nickname": "SomeNick", //The name the bot will have<br>
      "password": "password", //If the nick is registered, with this you can put the password in order for him to log in correctly through it, otherwise leave it like ""<br>
      "host": "irc.libera.chat", //The server the bot will be<br>
      "canal": "#someChannel" //The channel where the bot shall be<br>

<h1>Pirate Protocol</h1>
<h2>The main proposal and his objectives</h2>
<p>The idea of this proposal is to define the pirate protocol a protocol that is simple, lightweight, flexible, easy to implement, consume, and privacy friendly, or at least as much privacy friendly as a protocol can be, the objective of this protocol is to be able to present content (in the laxest definition of the term) in the biggest variety of client applications possible than are tecnically able to run with it, being the case they were designed for that or not.</p>
<br>
<h2>What is the reason of this?</h2>
The main idea of this and main benefit is that, by his own definition it will do several things:<br>
<ul>
<li>1) It will allow clients from different services to consume content through it from another services.</li><br>
<li>2) It will allows clients from different services to coordinate connections to specific uses in other services through it, according to availability.</li><br>
<li>3) Allows both ways of service use and content consumption; in other words it allows easy direct encapsulation and consumption of content through it to be used in another service, and/or make another service (A) being able to be used or his content consumed by users in another service (B)</li><br>
</ul>
<h2>Why not using GOPHER, GEMINI or HTTP(S) for this?</h2>
Because the first is not that easy to implement in other services in a safe way that do not impact the other service, and both, the first and the second are mostly for presenting data mostly if not completely, they are both lacking in any significant or flexible enough way to allow for easy user interaction, meanwhile the third is too bloated, unsafe, privacy unfriendly and required too much permits, resources and client colaboration to being able to work or be implemented properly, part of what does a client - server approach to things.
<br>
<h2>How does that work?</h2>
This protocol works a little different than the rest of the text-based internet protocols, it is design to, instead of working with the regular client-server structure, it works with a client-middleware-server structure in mind, the idea is keeping a client agnostic approach as much as possible and assuming most of the time other services are gonna be the channel from which a client will connect to your middleware and consume his content, in other words you are not gonna receive that many direct client connections with this although you absolutely could, the pirate-protocol is not designed against that use-case but is not his main use case, because flexibility is paramount.<br>
<br>
Being the case than the protocol is a way to present wherever content from another service or services to a client in a different service (as it can be presenting GOPHER/GEMINI to a client in IRC through IRC), you need to assume you know nothing about it (the client) or know too little about it (only he is gonna be consuming the content through IRC) to design specifically for it, therefore you need to keep it as flexible as possible. As such, you need to consider than you are gonna be implementing this protocol in the middleware to be served to the clients interacting with it, whereas that be direct interaction or most probably an indirect interaction it depends of your implementation, but the idea of this protocol is helping you with that.<br>
<br>
<h3>Approach</h3>
For this reason this is a text-based backend-enforced protocol, because the idea is than this protocol is presented from a middleware which is who makes the consumption of the other services possible to the clients of another different service, and in order to give some order the the data from that other services is that it present them as it does.<br>
<br>
<h4>Text-based</h4>
Indeed, as its neccesary to keep a client agnostic approach, you are kinda forced to use the most universal thing than any client can in the best way you can and that is text, this is because mostly every service than allows communication allows for this, but sadly, that cause than you cannot present images directly, but depending on your implementation they are alternative ways to solve that issue.<br>
<br>
<h4>Back-End enforced</h4>
As the approach ask to be client agnostic, and as we cannot use client-based code because of it (like javascript for instance), we are forced to do everything only from the backend, the middleware shall be the one presenting a file with the info to the client as the protocol required, but for certain it doesn't required to be a simple reading and redirecting, it could very well modify the content of what its sending in order to dynamize the content in the same way than, for instance, PHP forums worked back in the day, presenting all the old posts about a topic than used to exist.<br>
<br>
<h4>Privacy Focus</h4>
As the approach demands simplicity and such simplicity is required to being able to keep the client agnostic and the flexibility than allows that, knowing than part of that is keeping only what you can do with the backend, it comes to reason than the Privacy approach shall be simple too, and the ways to transmit communications shall be keep simple too, that is why we are using a TLC/SSH approach for both, consume the content and also communicate the data to the middleware but not in a single persistant connection but like connections than are closed at the moment the receiver end of anything you are sending finish to receive what it supposed to recieve, being that the client, being that the middleware or so. That being said persistan communications can be allowed only if the use case requires them and they cannot be avoided.<br>
<br>
<h4>Flexibility</h4>
As the middleware will need to work in several services, the idea of this protocol is taking a flexible approach with his links with such services, as such there is a three step approach to regulate any interaction with the clients through any service.<br>
<br>
<h5>What is the three steps approach?</h5>
The three steps approach establish that, in any interaction other than present text-based content like, for instance a file, an image or so, the answer from the middleware should be attempted using this three steps way, and this shall be done in such a way than if the first approach suggested cannot be done or fails the next one have to be tried:<br><br>
<ul>
<li> 1) First, try to use the ways the protocol of that services has integrated on itself; if for instance a request for a file is done from a IRC client through a connection established where the middleware is working as a bot, the right approach is trying to send the file as DCC or in the case of an image, sending the image maybe as a file or link, like it would be handle in IRC itself.</li><br>
<li> 2) If the approach before fails, then use the TAVERN approach; Basically the middleware gives the client an address or similar to go fetch wherever file or image he was looking for in wherever protocol the guy hosting it in that direction preffered, it could be FTP, it could be HTML, it could be Usenet, it could be an ip to make a CURL call, etc. The point is, in this case you are directing the user to where he can get the content he's trying to get than his client cannot handle by itself, more on the way of implementation of TAVERN later in the technical implementation part of this document.</li><br>
<li> 3) If the approach before mentioned fails because there is nothing externally handling that, or privacy or another something requires for it, then the middleware will direct to another pirate-protocol middleware which will handle the issue as a pirate-protocol request by his own ways than will be talked a little more in detail about later in the technical specifications document, one of the reasons to avoid doing the handling directly is trying to keep the UNIX philosophy, than this protocol adheres whenever possible, you of course could use for this deployiment of file/images or so the middleware itself than is receiving the call, but that is discouraged in favour of keep things simple and to minimize security issues, however if its required, you can absolutly do that, don't feel you are going against this protocol if you are doing the direct approach in this point.</li><br>
</ul>
<br>
<h2>How can i implement this if X happend? (being X a limit case)</h2>
When in doubt consider this, as a rule of thumb, flexibility and client agnosticism take precedent before servicing quality, and also, every time its possible, using the ways the service protocol than is working as medium or channel has established for the distribution of something take precedent over the pirate-protocol ways.<br>
<br>
To give an example of that consider this very project where you can find in this very text, this bot, than act as a middleware to present GOPHER to an IRC user put the GOPHER messages surrounded by the IRC protocol messages required to present it, and as such you need to consider than the pirate-protocol will probably be handle through other protocols most of the time as GOPHER is handle in this very project, and that is the idea, so if a middleware connected to IRC need to present content in pirate-protocol this pirate-protocol content would be woven in IRC protocol for sure, as such, as is the case of a middleware working as an IRC bot, if you want to make available a file you shall probably consider using DCC to deal such file to the client which is the IRC way to deal with it even that would be the third case in the three steps approach.<br>
<br>
This is true only because, as the middleware is acting as a bot, you know the client will be an IRC client and your channel (your way to communicate with the client) will be an IRC server, therefore flexibility and client agnosticism takes precedent because the medium requires such flexibility, in this case, the flexibility requires the use of IRC to present the pirate-protocol content to the client, than in this context we know it will be an IRC client.<br>
<br>
<h2>Technicall specification document</h2>
<h3>Definitions and Conventions</h3>
As this protocol have Pirate in the name we are taking port and nautic references for denominations in, somewhat functional-equivalent ways, as such:<br>
<br>
<ul>
  <li><b>Wharf:</b> A Wharf is basically the full complete address than you are about to see or you are currently          seing, for instance: "pirate://example.org/foo/bar" It can also mean the file itself than have that               content. Its called like that because a wharf is the place where ships dock (as, you "docked" in there)           and load/unload cargo and/or passengers.</li>
  <li><b>Astrolabe:</b> An astrolabe is basically a link in web lingo, thats it, called like that because of the           instruments old sailors used to know his location using the stars.</li>
  <li><b>Ramp:</b> A ramp is basically the button than send some data to a Wharf, similar in appearance to the form interaction from html but technically totaly different, they are called like this because of the Ro-Ro Ramps than are used in some ships to allow vehicles enter and exit to facilitate the load/unload of cargo.</li>
</ul>
<br>
There is no breakline or similar concept in this protocol designed inside it, it is expected however than the middleware serves one line at the time to help flexibility but exceptions can be done if the services that is used as a channel requires it, otherwise the time between servings or the max size of a line it's up to every middleware.<br>
<br>
That being said, if it were necessary, \n or any other linebreaker could work, provided the service used as the way of communication require as such.<br>
<br>
<h3>Elements</h3>
In Pirate-Protocol elements distinguish between only two types
<ul>
  <li>Informative elements<br>Which are those who show information.</li><br>
  <li>Interatible elements<br>Which are those who are supposed to be interacted with</li><br>
</ul>
<h4>Informatible lines</h3>
This are lines than, or bring some information to the client as for example language and text type, or are the content itself presented to the user, the types of this are as they are presented here:.<br>
<br>
<ul>
  <li>## For comments</li>
  <li>lang="es" (For the language, optional)</li> 
  <li>1 text/pirate (To indicate than the document being received is a pirate one, it come as a text in pirate mode, it also includes the numcode of the result of the transaction, more on that later)</li>
</ul>

<h4>Interactible lines</h3>
<h5>Common elements</h5>
All Interactible lines shall begin with [#] being '#' being a number only used by that line for identification purposes, after a whitespace shall contain the definer of which type of interactable, the possible types are as follow:<br>
<ul>
  <li> "=>" (Links, here called Astrolabe)</li>
  <li> "<°>" (Buttons, here called "Ramps")</li>
  <li> ">|<" Warehouse (Anwers given by a call, as a table or such could be)</li>
</ul>
<br>
So they shall see as follows:<br>
<br>
    
    [#][<space>][TYPE-OF-INTERACTABLE][<space>][INTERACTABLE-RELEVANT-INFO]
      
<br>
Which, taking as an example of links "=>" this is how an interactable line shall look like:<br>
<br>
      
    [#] =>[<space>][URL][<space>||<space><USER-FRIENDLY LINK NAME>]
    
<br>
Where <space> is any non-zero number of spaces or tabs and square brackets indicate that the enclosed content is optional. Or in other words, when written on a file (i.e. the wharf) it shall see something like this cases:
<br><br>
   
    [#] => pirate://example.org/
    [#] => pirate://example.org/ || An example link
    [#] => pirate://example.org/foo ||	Another example link at the same host
    [#] => foo/bar/taz.txt || A relative link
    [#] => gopher://example.org:70/1 || A gopher link

<br><br>
Which shall be rendered something like this:
<br><br>
   
    [1] => pirate://example.org/
    [2] => An example link
    [3] => Another example link at the same host
    [4] => A relative link
    [5] => A gopher link

<br><br>
The reason we are writting [#] instead of the number directly is because the middleware, when interpreting the wharf will be the one who will define how to order them and any other of the interactables that is to give more flexibility to the middleware allowing it for different approaches to interact with the wharf. That is the same reason we are using "||" as a separator too because that way is easier to code the middleware in most back-end languages than have any integrated tools to work with strings.<br>
<br>
<h5>Specifically about the buttons</h5>
<br>
Meanwhile the astrolabes works as intended with just a minimal interaction, (basically a signal than doesn't require any more data than reffer your intention of execute it) the Ramps (buttons) by the other hand do sometimes require extra data to properly work.<br>
<br>
For this reason we will first talk about how to properly write them in the wharf (file), then the format to send the data and the format to know how to wait for it in different context.<br>
<br>
As you may rememer the strucutre of presentation of any interactable is like this:<br>
<br>
    
    [#][<space>][TYPE-OF-INTERACTABLE][<space>][INTERACTABLE-RELEVANT-INFO]
      
<br>
Which for the ramps is "<°>" this is how an interactable line shall look like:<br>
<br>

    [#] <°> [<space>][ID_BUTTON]||[Text To Show the Button][<space>||<space><ID_ANSWER>]
    
<br>
Let's analize it a little bit: first we have the Interactable ID "[#]" to be defined by the middleware, and the mandatory ramp sign "<°>" indicating which type of interactable is, after that comes the specific ID_BUTTON which is a unique ID specifically designed for that button in particular and, instead of being setted by the middleware as he preffer to talk with the client, it will connect the middleware with the server hosting the wharf, that is because it has his own ID separated from the interactables ones, next to it the text than shall render just next to the sign of the type and after that, separated by a "||" but in an optional way, there is the ID_ANSWER which will tell the middleware where shall present the answer to that request in the wharf, if it must of course, they however rendered, and taking by example the last line, shall be seen something like this:<br>
<br>

    [3] <°> Text To Show the Button
    
<br>
How it is used then?<br>
The middleware or client shall send something like this through wherever service or way it shall to the server<br>
<br>

    <°> ButtonName || Some data to send, this can use all the remaining space
    
<br>
If there is an answer to be waited at, this shall be placed in a warehouse somewhere like this:<br>
<br>

    [#] >|< [<space>][ID_ANSWER][<space>||<space><Content Of The Answer>]
    
<br>
Please notice than warehouse are optionally interactables, that does mean than a warehouse can be used to only show data instead, as such, this both examples are valid being the first interactable and the second not, and both represent the before thing well enough:<br>
<br>

    [4] >|< Answer Received!
    >|< Answer Received!
    
<br>
<h3>Codes</h3>
<h4>How the codes works</h4>
The answer codes in the pirate-protocol are done in a composed way instead of the relatibly common pre-establish way other protocols use, the way to build a code answer in the pirate protocol is pretty simple. A complete list of the relevant codes will be presented after the explanation of how this codes work:

The code for success is:

    1

Which is also equivalent to:

    01 or 10

The code for failure is:

    2

Which can also be written as:

    02 or 20

From them on you can build your answers, for instance if you want to give successful create you add the number of the action next to the result of the action, as such:

    11

And if the creation failed, knowing than 1 is creation, you will write:

    21

Simple, right? the first number define the result in general terms, the second define the specific action related to that result, there is a little more tho.

Lets supposed you have a failure but from the server side, something like the fact than that service is not implemented, in that case you dont have a failure of the action, but a server error, therefore you write it as such:

    201

Which means than there was a fail (2) but not in the intended action (0) but in the server answer, as such, the answer is 201 in the example because its saying "There was an error but it wasnt about the action (because of the 0) but in the server, which reported than that method is not implemented".

<h5>First Digit (General Result)</h5>
<table>
  <tr>
    <th>Code</th>
    <th>Meaning</th>
  </tr>
  <tr>
    <td>1</td>
    <td>Success</td>
  </tr>
  <tr>
    <td>2</td>
    <td>Failure</td>
  </tr>
</table>

<h5>Second Digit (Action)</h5>
<table>
  <tr>
    <th>Code</th>
    <th>Meaning</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>0</td>
    <td>Not Related to Action</td>
    <td>The answer is not directly related to the action attempted or performed.</td>
  </tr>
  <tr>
    <td>1</td>
    <td>Created</td>
    <td>The request succeeded, and a new resource was created as a result.</td>
  </tr>
  <tr>
    <td>2</td>
    <td>Accepted</td>
    <td>The request has been received but not yet acted upon. It is noncommittal, since there is no way in the pirate-protocol to later send an asynchronous response indicating the outcome of the request. It is intended for cases where another process or a different server than the one passed through the middleware handles the request, or for batch processing.</td>
  </tr>
  <tr>
    <td>3</td>
    <td>Non-Authoritative Information</td>
    <td>This response code means the returned metadata is not exactly the same as is available from the origin server, but is collected from a local or a third-party copy. This is mostly used for mirrors or backups of another resource. Except for that specific case, the 1 Success response is preferred to this status.</td>
  </tr>
  <tr>
    <td>4</td>
    <td>No Content</td>
    <td>There is no content to send for this request.</td>
  </tr>
  <tr>
    <td>5</td>
    <td>Partial Content</td>
    <td>This response code is used in response to a range request when the middleware has requested a part or parts of a resource.</td>
  </tr>
  <tr>
    <td>6</td>
    <td>Multi-Status</td>
    <td>Conveys information about multiple resources, for situations where multiple status codes might be appropriate.</td>
  </tr>
  <tr>
    <td>7</td>
    <td>Bad Request</td>
    <td>This code indicates that the middleware receive from the server the answer than it would not process the request due to something the server considered to be a client error. The reason is typically due to malformed request syntax, invalid request message framing, or deceptive request routing.

Clients that receive this response from the middleware should expect that repeating the request without modification will fail with the same error.</td>
</tr>
</table>

<h5>Third Digit (Server Error Responses)</h5>
<table>
  <tr>
    <th>Code</th>
    <th>Meaning</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>1</td>
    <td>Internal Server Error</td>
    <td>The server has encountered a situation it does not know how to handle. This error is generic, indicating that the server cannot find a more appropriate status code to respond with.</td>
  </tr>
  <tr>
    <td>2</td>
    <td>Not Implemented/Not Found</td>
    <td>The request method is not supported by the server and cannot be handled or couldn't be found.</td>
  </tr>
  <tr>
    <td>3</td>
    <td>Bad Gateway</td>
    <td>This error response means that the server, while working as a gateway to get a response needed to handle the request, got an invalid response.</td>
  </tr>
  <tr>
    <td>4</td>
    <td>Service Unavailable</td>
    <td>The server is not ready to handle the request. Common causes are a server that is down for maintenance or that is overloaded. Note that together with this response, a user-friendly page explaining the problem should be sent. This response should be used for temporary conditions.</td>
  </tr>
  <tr>
    <td>5</td>
    <td>Gateway Timeout</td>
    <td>This error response is given when the server is acting as a gateway and cannot get a response in time.</td>
  </tr>
  <tr>
    <td>6</td>
    <td>Variant Also Negotiates</td>
    <td>The server has an internal configuration error: during content negotiation, the chosen variant is configured to engage in content negotiation itself, which results in circular references when creating responses.</td>
  </tr>
  <tr>
    <td>7</td>
    <td>Insufficient Storage</td>
    <td>The method could not be performed on the resource because the server is unable to store the representation needed to successfully complete the request.</td>
  </tr>
  <tr>
    <td>8</td>
    <td>Loop Detected</td>
    <td>The server detected an infinite loop while processing the request.</td>
  </tr>
  <tr>
    <td>9</td>
    <td>Network Authentication Required</td>
    <td>Indicates that the client needs to authenticate to gain network access.</td>
  </tr>
</table>
