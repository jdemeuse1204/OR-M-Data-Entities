# OR-M-Data-Entities
Speedy Object Relational Mapper

OR-M Data Entities
Overview:  <br><br>
This solution is a micro Object-Relational Mapper aimed at speed.  Others out there are bulky and slow, unlike ORM Data Entities.  The catch is there is less "micro managing" of code going on, which makes the framework much faster.  

OR-M Data Entities is now even better. Support was added for ForeignKeys, Pseudo Keys, ReadOnly Tables, and VIEWS!  Yes thats right, Views now exist!  When writing the new code I realized some models that have foreign keys can get pretty big and you might not want to always select everything.  Sure you could shape your data with a select, but that can be a lot of code for bigger models.  Enter views!  Just put the ViewAttribute on each Foreign Key class and specify which view you want it to be a part of.  Then in the context do FromView<T>(string viewId) to select your data. 

###Documentation Link:

http://ormdataentities.com/

Issues/Questions/Comments:

Mail To - james.demeuse@gmail.com
