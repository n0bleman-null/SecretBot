CREATE TABLE Chat(
    Id int not null primary key generated always as identity ,
    ChatId bigint not null unique
);

CREATE TABLE Player(
    Id int not null primary key generated always as identity ,
    PlayerId bigint not null unique
);

CREATE TABLE Game(
    Id int not null primary key generated always as identity ,
    ChatId int not null references Chat(Id) on delete cascade,
    Winner boolean NOT NULL -- 0 as liberal, 1 as fascist
);

CREATE TABLE PLayerGame(
    Id int not null primary key generated always as identity ,
    PlayerId int not null references Player(Id) on delete cascade ,
    GameId int not null references Game(Id) on delete cascade ,
    Role boolean NOT NULL, -- 0 as liberal, 1 as fascist-
    constraint OnePlayerPerGame unique (PlayerId, GameId)
);
