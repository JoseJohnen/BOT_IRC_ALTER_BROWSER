<h1>BOT IRC ALTER BROWSER</h1>

An IRC bot created for allowing the browsing of Gopherspace and Geminispace from IRC as a proof of concept for the idea of "middleware browser".

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
<p>The idea of this proposal is to define the pirate protocol a protocol that is simple, lightweight, flexible, easy to implement and consume, and privacy friendly as much as a protocol can be, the objective of this protocol is to be able to present content (in the laxest definition of the term) in the biggest variety of client applications possible than are tecnically able to do that being the case they were designed for that or not.</p>
<br>
<h2>What is the reason of this?</h2>
The main idea of this and main benefit is that, by his own definition it will do several things:<br>
<ul>
<li>1) It will allow clients from different services to consume content through it from another services.</li><br>
<li>2) It will allows clients from different services to coordinate connections to specific uses in other services through it, according to availability.</li><br>
<li>3) Allows both ways of service use and content consumption; in other words it allows easy direct encapsulation and consumption of content through it to be used in another service, and/or make another service (A) being able to be used or his content consumed in another service (B)</li><br>
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
<b>Coming Soon...</b>
