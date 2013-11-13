using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunt_the_Wumpus {
    public class Program {
        //main entry point to start the game
        static void Main(string[] args) {
            //decides whether game is over or not
            bool play = false;
            Map cave;
            Player player;
            Wumpus wumpus;
            SuperBats bats;
            Pit pitA;
            Pit pitB;

            //initializes a new game
            Initialize(ref play, out cave, out player, out wumpus,
                       out bats, out pitA, out pitB);

            //the game loop
            while(play) {
                Draw(ref cave);
                Update(ref play, ref cave, ref player, ref wumpus,
                       ref bats, ref pitA, ref pitB);
            }
        }

        //initializes a random new game
        public static void Initialize(ref bool play, out Map cave, out Player player, 
                                      out Wumpus wumpus, out SuperBats bats, out Pit pitA,
                                      out Pit pitB) {
            //will start the main game loop later
            play = true;

            //initialize object locations
            int[]locations = generateLocations();

            //creates a new map
            cave = new Map(20, locations[0],  locations[1], locations[2],
                               locations[3], locations[4]);

            //creates the player
            player = new Player(cave.Oloc[0]);
            //creates the wumpus
            wumpus = new Wumpus(cave.Oloc[1]);
            //creates a group of super bats
            bats = new SuperBats(cave.Oloc[2]);
            //creates a pit
            pitA = new Pit(cave.Oloc[3]);
            //creates a pit
            pitB = new Pit(cave.Oloc[4]);

            Console.WriteLine("HUNT THE WUMPUS - A new cave has been generated");
        }
        
        //console I/O
        public static void Draw(ref Map cave) {
            warning(ref cave);
            Console.WriteLine("You are in room " + (cave.Cloc[0] + 1));
            Console.WriteLine("Tunnels connect to " + cave.Rooms[cave.Cloc[0]].Adj[0] + " " + cave.Rooms[cave.Cloc[0]].Adj[1] +
                              " " + cave.Rooms[cave.Cloc[0]].Adj[2]);
            Console.WriteLine("Please (S)hoot or (M)ove");
        }
        
        //update on the character's status
        public static void Update(ref bool play, ref Map cave, ref Player player, ref Wumpus wumpus,
                                ref SuperBats bats, ref Pit pitA, ref Pit pitB) {

            //wumpus logic
            if(wumpus.Awake) {
                wumpus.move(cave);

                if(wumpus.Room == player.Room) {
                    player.Alive = false;
                    play = false;
                }
            }

            //player logic
            if(player.Alive) {
                bool turn = true;
                //continues checking input until it is valid
                while(turn) {
                    //gets input
                    String move = Console.ReadLine();

                    //parses input
                    //if the player chose to move
                    if(string.Compare(move, 0, "M", 0, 1, true) == 0) {
                        //prompts for room number
                        Console.WriteLine("Tunnels connect to " + cave.Rooms[cave.Cloc[0]].Adj[0] + " " + cave.Rooms[cave.Cloc[0]].Adj[1] +
                                          " " + cave.Rooms[cave.Cloc[0]].Adj[2]);
                        Console.WriteLine("What room would you like to move to?");
                        move = Console.ReadLine();

                        //parses the room number
                        int room;
                        int.TryParse(move, out room);
                        bool valid = true;

                        //checks input against the adjacent rooms
                        for(int i = 0; i < 3; i++) {
                            if(room == cave.Rooms[cave.Cloc[0]].Adj[i]) {
                                player.move(ref cave, room);
                                break;
                            } else if(i == 2) {
                                Console.WriteLine("Not possible");
                                Console.WriteLine("Please (S)hoot or (M)ove");
                                valid = false;
                            }
                        }
                        if(!valid) {
                            continue;
                        }
                    } else if(string.Compare(move, 0, "S", 0, 1, true) == 0) {
                        player.shoot(ref cave, ref wumpus);
                    } else {
                        //invalid input, ask for reentry of input
                        Console.WriteLine("Invalid Input");
                        Console.WriteLine("Please (S)hoot or (M)ove");
                        continue;
                    }
                    turn = false;
                }
            } else {
                Console.WriteLine("The Wumpus ate you! Game Over!");
            }
        }

        //update on the character's status
        public static void warning(ref Map cave) {

        }

        //generates random numbers for the object locations
        //no two objects should be in the same room
        public static int[] generateLocations() {
            //holds the random numbers generated
            int[] numbers = new int[5];
            //new random generator
            Random genRandom = new Random();

            //makes sure there are no duplicate randoms stored
            for(int i = 0; i < numbers.Length; i++) {
                bool found = false;
                int random = 0;

                //the random number can't be 0
                while(random == 0) {
                    random = genRandom.Next(1, 101) / 5;
                }

                //prevents duplicates, if a duplicate is found, the bool is flipped
                for(int j = 0; j < i; j++) {
                    if(random == numbers[j]) {
                        found = true;
                    }
                }

                //if a duplicate was found, restart the loop with a new random
                //otherwise, store the random
                if(found) {
                    continue;
                } else {
                    numbers[i] = random;
                }
            }

            return numbers;
        }


        /*********************
       ******** CLASSES *******
        *********************/

        //a game map, each map created is a unique game
        public class Map {
            //the rooms in the map, stored in an array
            private Room[] rooms;
            public Room[] Rooms {
                get {return rooms;}
                set {rooms = value;}
            }

            //the original locations of objects in the map
            //reused if the player wants to use the same map
            private int[] oLoc = new int[5];
            public int[] Oloc {
                get {return oLoc;}
                set {oLoc = value;}
            }
            
            //the current locations of objects in the map
            //changes every turn
            private int[] cLoc;
            public int[] Cloc {
                get { return cLoc; }
                set { cLoc = value; }
            }

            //constructor
            public Map(int size, int player, int wumpus, 
                       int bats, int pitA, int pitB) {
                //generates rooms and sets up mapping
                rooms = new Room[size];
                setMap();

                //gets the object locations and stores them
                oLoc[0] = player;
                oLoc[1] = wumpus;
                oLoc[2] = bats;
                oLoc[3] = pitA;
                oLoc[4] = pitB;

                //marks the rooms that have objects
                for(int i = 0; i < oLoc.Length; i++) {
                    rooms[oLoc[i]].Noun = true;
                }

                //copies to current location array
                cLoc = oLoc;
            }

            //sets up room array and room adjacencies
            private void setMap() {
                //the room setup, each item in the map array is a room
                rooms[0] = new Room(5, 8, 2);
                rooms[1] = new Room(1, 10, 3);
                rooms[2] = new Room(2, 12, 4);
                rooms[3] = new Room(3, 14, 5);
                rooms[4] = new Room(4, 6, 1);
                rooms[5] = new Room(15, 5, 7);
                rooms[6] = new Room(6, 17, 8);
                rooms[7] = new Room(7, 1, 9);
                rooms[8] = new Room(8, 18, 10);
                rooms[9] = new Room(9, 2, 11);
                rooms[10] = new Room(10, 19, 12);
                rooms[11] = new Room(11, 3, 13);
                rooms[12] = new Room(12, 20, 14);
                rooms[13] = new Room(13, 4, 15);
                rooms[14] = new Room(14, 16, 6);
                rooms[15] = new Room(15, 17, 20);
                rooms[16] = new Room(16, 7, 18);
                rooms[17] = new Room(17, 9, 19);
                rooms[18] = new Room(20, 18, 11);
                rooms[19] = new Room(16, 13, 12);
            }
        }
        
        //an individual room in the map
        public class Room {
            //an array that holds the adjacent rooms
            private int[] adj = new int[3];
            public int[] Adj {
                get { return adj; }
                set { adj = value; }
            }

            //booleans to help check what is in a room
            private bool noun = false;
            public bool Noun {
                get { return noun; }
                set { noun = value; }
            }

            //the constructor for a room, sets up adjacencies
            public Room(int a, int b, int c) {
                //sets up the adjacent room numbers
                adj[0] = a;
                adj[1] = b;
                adj[2] = c;
            }
        }

        //every other object/noun in the game exists in a room
        public class Noun {
            //the current room an object is in
            private int room;
            public int Room {
                get { return room; }
                set { room = value; }
            }

            //constructor
            public Noun(int room) {
                this.room = room;
            }
        }

        public class Live:Noun {
            //the status of a live object
            //for the player and wumpus
            private bool alive;
            public bool Alive {
                get { return alive; }
                set { alive = value; }
            }

            //constructor
            public Live(int room)
                : base(room) {
                alive = true;
            }
        }

        //the player object
        public class Player:Live {
            //the room the arrow is in
            private int arrow = 0;
            public int Arrow {
                get { return arrow; }
                set { arrow = value; }
            }
            //once an arrow is shot this is set to true
            private bool shot = false;
            public bool Shot {
                get { return shot; }
                set { shot = value; }
            }

            //constructor
            public Player(int room)
                : base(room) {
            }

            //move logic
            public void move(ref Map cave, int newRoom) {
                cave.Rooms[Room - 1].Noun = false;
                Room = newRoom;
                cave.Cloc[0] = Room;
                cave.Rooms[Room - 1].Noun = true;
            }

            //shoot
            public void shoot(ref Map cave, ref Wumpus wumpus) {
                //arrow prompt
                Console.WriteLine("You shot your crooked arrow!");

                //wakes the wumpus on the first shot
                if(!Shot) {
                    Shot = true;
                    Console.WriteLine("The wumpus has awaken!");
                    wumpus.Awake = true;
                }

                //start the arrow at the player's location
                Arrow = cave.Cloc[0];

                //logic for arrow moving, path updates
                for(int i = 5; i > 0; i--) {
                    //console I/O
                    Console.WriteLine("Your arrow is in room " + Arrow);
                    Console.WriteLine("Tunnels connect to " 
                                      + cave.Rooms[Arrow].Adj[0] + " " 
                                      + cave.Rooms[Arrow].Adj[1] + " " 
                                      + cave.Rooms[Arrow].Adj[2]);
                    Console.WriteLine("What room do you want your arrow to travel to?");
                    String move = Console.ReadLine();

                    //parse input
                    int newRoom;
                    int.TryParse(move, out newRoom);
                    bool valid = true;

                    //checks input against the adjacent rooms
                    for(int j = 0; j < 3; j++) {
                        if(newRoom == cave.Rooms[cave.Cloc[0]].Adj[j]) {
                            //updates the arrow's location
                            Arrow = newRoom;
                            break;
                        } else if(i == 2) {
                            Console.WriteLine("Not possible");
                            Console.WriteLine("Please choose a correct path.");
                            valid = false;
                        }
                    }
                    if(!valid) {
                        continue;
                    }

                    //checks to see if wumpus is hit
                    if(Arrow == wumpus.Room) {
                        Console.WriteLine("You've slain the Wumpus!");
                        wumpus.Alive = false;
                        break;
                    }
                    
                    //tells player how many moves the arrow has left
                    Console.WriteLine("You arrow can travel " + i + " more rooms.");
                }

                Console.WriteLine("Your arrow didn't hit anything.");
            }
        }

        //the wumpus object
        public class Wumpus:Live {
            //the wumpus' sleep status
            private bool awake = false;
            public bool Awake {
                get { return awake; }
                set { awake = value; }
            }

            public Wumpus(int room)
                : base(room) {
            }

            public void move(Map cave) {
                //generates a random number
                Random genRandom = new Random();
                int random = genRandom.Next(1, 101);

                //75 percent chance that the wumpus moves
                if(random > 25) {
                    //the wumpus moves
                    //randomly move to next room
                    random = genRandom.Next(1, 4);

                    cave.Rooms[Room - 1].Noun = false;
                    Room = random;
                    cave.Cloc[1] = Room;
                    cave.Rooms[Room - 1].Noun = true;
                }
            }
        }

        //superbat groups
        public class SuperBats:Live {
            //constructor
            public SuperBats(int room)
                : base(room) {
            }
        }

        //pit objects
        public class Pit:Noun {
            public Pit(int room)
                : base(room) {
            }
        }
    }
}