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
<p>The idea of this proposal is to have a protocol that is simple, easy to implement, and privacy friendly as much as it can a protocol be, the objective of this protocol is to be able to present content (in the laxest definition of the term) in the most possible "client" aplications than are tecnically able to do that</p>




