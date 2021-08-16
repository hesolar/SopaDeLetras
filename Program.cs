using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//3h y 30

namespace SopaDeLetras {
    class Program {

        public static Dictionary<string,int> aparicionesPalabras = new ();
        static async Task Main( string[] args ) {
            

            StreamReader lectorSopaDeLetras = new ("sopag.txt");
            var sopaLecturaAsincrona = lectorSopaDeLetras.ReadToEndAsync();
            StreamReader lectorPalabras = new ("palabrasg.txt");
            var palabrasAsincronas = lectorPalabras.ReadToEndAsync();
            
            Console.WriteLine("La sopa para leer es:");
            List<String> sopa = sopaLecturaAsincrona.ContinueWith(x => x.Result.Split(Environment.NewLine).ToList()).Result;
            sopa.ForEach(x => Console.WriteLine(x));
            int numeroDeFilas = sopa.Count;

            var matriz = CrearMatrizAsync(sopa,numeroDeFilas);
            

            List<String>palabras=palabrasAsincronas.ContinueWith(x => x.Result.Split("\r\n").ToList()).Result;
             
            


            List<String> palabrasQueAparecen = WordsInMatrix(palabras,matriz.Result.Keys.ToList());
            palabrasQueAparecen.ForEach(x => aparicionesPalabras[x] = 0);
           
            palabrasQueAparecen.AsParallel().ForAll(x => ThreadProcess(x,matriz.Result));

            aparicionesPalabras.Keys.ToList().ForEach(x => Console.WriteLine(x + " aparecen : "+ aparicionesPalabras[x]));
            
            Console.Write("seconds:" + (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()));


        }

      
        /*
        *Apuntes:
        * 
        * 
        *Char.IsLetter
        * Environment.NewLine
        * streamReader.ReadToEnd()
        * streamReader.ReadLine()
        * 
        * Ctrl + M, Ctrl + M	Collapses all code outlining
        * Ctrl + M, Ctrl + L	Expands all code outlining
        * Ctrl + K, Ctrl + D    Format
        * 
        */

        public static void ThreadProcess( String palabra,Dictionary<string,List<Tuple<int,int>>> matriz ) {
            int i, dirx, diry, x, y;


            int sizeWord = palabra.Length;
            i = 0;
            dirx = 0;
            diry = 0;

            String letra = palabra[0] + "";

            List<Tuple<int,int>> posicionesLetra = matriz[letra + ""];
            if( posicionesLetra.Any() ) {
                foreach( Tuple<int,int> posicionLetra in posicionesLetra ) {
                    i = 1;
                    (x, y) = posicionLetra.ToValueTuple<int,int>();
                    String letra2 = palabra[i] + "";
                    List<Tuple<int,int>> posicionesSiguienteLetra = matriz[letra2];
                    List<Tuple<int,int>> adyacentes = posicionesSiguienteLetra.Where(t => (x + 1) == t.ToValueTuple().Item1 || (x - 1) == t.ToValueTuple().Item1 || (x) == t.ToValueTuple().Item1).Intersect(
                    posicionesSiguienteLetra.Where(t => (y + 1) == t.ToValueTuple().Item2 || (y - 1) == t.ToValueTuple().Item2 || (y) == t.ToValueTuple().Item2)).ToList();
                    foreach( var t in adyacentes ) {
                        i = 2;
                        dirx = t.ToValueTuple().Item1 - x;
                        diry = t.ToValueTuple().Item2 - y;
                        (int resx, int resy) = t.ToValueTuple();

                        while( i < sizeWord - 1 && i != 0 ) {
                            resx += dirx;
                            resy += diry;
                            letra = palabra[i] + "";
                            var encontrada = matriz[letra].Where(w => (resx, resy) == w.ToValueTuple()).ToList();
                            i = encontrada.Any() ? i + 1 : 0;
                        }
                        if( i != 0 ) aparicionesPalabras[palabra] = aparicionesPalabras[palabra] + 1;
                    }
                }
            }
        }

        public static async Task<Dictionary<string,List<Tuple<int,int>>>> CrearMatrizAsync( List<String> sopa,int numeroFilas ) {
            Dictionary<string,List<Tuple<int,int>>> matriz = new ();
            int i;
            foreach( string line in sopa ) {

                var fila = sopa.IndexOf(line);
                var letters = line.Split(' ').Take(numeroFilas).ToList();
                i = 0;
                foreach( string letter in line.Split(' ') ) {

                    //Cadena vacia
                    if( letter.Length > 0 ) {
                        var columna = letters[i];
                        Tuple<int,int> t = new (fila,i);
                        if( matriz.ContainsKey(letter) ) {
                            List<Tuple<int,int>> l = matriz[letter];
                            l.Add(t);
                            matriz[letter] = l;
                        }
                        else {
                            List<Tuple<int,int>> nuevaLista = new ();
                            nuevaLista.Add(t);
                            matriz.Add(letter,nuevaLista);
                        }

                    }
                    i++;
                }
            }
            return matriz;
        }

        public static void MostrarMatriz( Dictionary<string,List<Tuple<int,int>>> matriz ) {
            foreach( var v in matriz.Keys ) {
                Console.WriteLine(v);
                foreach( var el in matriz[v] )
                    Console.WriteLine(el);
                Console.WriteLine();
            }
        }

        public static List<String> WordsInMatrix( List<String> words,List<String> keymatrix ) {
            //return words.Where(x => x.ToCharArray().ToList().TrueForAll(z => keymatrix.Contains(z.ToString()))).ToList();
            return words.AsParallel().Where(x => x.ToCharArray().ToList().TrueForAll(z => keymatrix.Contains(z.ToString()))).ToList();
        }
    }
}