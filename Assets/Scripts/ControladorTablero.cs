using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorTablero : MonoBehaviour
{
    public Text lineas;
    public Text puntos;
    public Text maxPuntos;
    public Text textoPausa;
    public Image imagenSiguiente;
    public Sprite [] tipo = new Sprite[7];

    private const int FILAS = 21;
    private const int COLUMNAS = 12;

    GameObject[,] TableroVista = new GameObject[FILAS, COLUMNAS];

    private const int VACIO = 0;
    private const int PIEZA_ACTUAL = 2;
    private const int PIEZA_MUERTA = 1;

    private Pieza pieza;
    private ModeloTetris modelo;

    private bool actuar;
    private bool actuarGiro;
    private bool pausado;

    /*private Color[] colores
            = {
                Color.yellow, Color.cyan, Color.magenta, Color.green, Color.red, Color.white, Color.blue
            };*/
    private Color[] colores
            = {
                new Color32(237, 251, 16, 255), new Color32(16, 235, 251, 255), new Color32(194, 251, 16, 251),
                            new Color32(49, 251, 16, 255), new Color32(251, 16, 16, 255), new Color32(251, 112, 16, 255), new Color32(63, 16, 251, 255)
            };
    void Start ()
    {
        pieza = new Pieza(Random.Range(0, 7), Random.Range(0, 7));
        textoPausa.enabled = false;

        actuar = true;
        actuarGiro = true;
        pausado = false;

        modelo = ModeloTetris.getModelo();
        modelo.setMaxPuntuacion(PlayerPrefs.GetInt("Record", 0));

		for (int i = 0; i < FILAS; i++)
        {
            GameObject fila = this.transform.GetChild(i).gameObject;
            for (int j = 0; j < COLUMNAS; j++)
            {
                TableroVista[i, j] = fila.transform.GetChild(j).gameObject;
            }
        }
        InvokeRepeating("bajar", 1.0f, modelo.getVelocidad());
	}
	
	void Update ()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            izquierda();
            soltar();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            derecha();
            soltar();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            bajar();
            soltar();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            girar(1);
            soltarGiro();
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            girar(-1);
            soltarGiro();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            pausar();
        }

        dibujar();
    }

    public void trampa()
    {
        if(pausado)
        {
            pieza.setNextTipo(1);
            dibujar();
        }
    }

    public void pausar()
    {
        if(Time.timeScale == 1)
        {
            Time.timeScale = 0;
            textoPausa.enabled = true;
            pausado = true;
        }
        else if(Time.timeScale == 0)
        {
            Time.timeScale = 1;
            textoPausa.enabled = false;
            pausado = false;
        }
    }

    void dibujar()
    {
        modelo.comprobarLinea();
        modelo.limpirar(pieza);
        lineas.text = "Lineas: " + modelo.getLineas();
        puntos.text = "Puntos: " + modelo.getPuntuacion();
        maxPuntos.text = "Record: " + modelo.getMaxPuntuacion();
        imagenSiguiente.sprite = tipo[pieza.getNextTipo()];

        for (int i = 0; i < FILAS; i++)
        {
            for (int j = 0; j < COLUMNAS; j++)
            {
                if (j == 0 || j == COLUMNAS - 1 || i == FILAS - 1)
                {
                    TableroVista[i, j].GetComponent<Renderer>().material.color = Color.black;
                }
                else if (modelo.getValorTetris(i, j) == VACIO)
                {
                    TableroVista[i, j].GetComponent<Renderer>().material.color = new Color32(187, 199, 200, 255);
                }
                else if (modelo.getValorTetris(i, j) == PIEZA_MUERTA)
                {
                    TableroVista[i, j].GetComponent<Renderer>().material.color = new Color32(84, 94, 94, 255);
                }
                else
                {
                    TableroVista[i, j].GetComponent<Renderer>().material.color = colores[pieza.getTipo()];
                }
            }
        }
    }

    public void soltar()
    {
        actuar = true;
    }

    public void soltarGiro()
    {
        actuarGiro = true;
    }

    public void girar(int sentido)
    {
        if (!pausado)
        {
            if (actuarGiro)
            {
                if (pieza.comprobarGiro(sentido))
                {
                    pieza.girar(sentido);
                }
                actuarGiro = false;
            }
        }
    }

    public void derecha()
    {
        if (!pausado)
        {
            if (actuar)
            {
                if (pieza.comprobarDerecha())
                {
                    pieza.derecha();
                }
                actuar = false;
            }
        }
    }

    public void izquierda()
    {
        if (!pausado)
        {
            if (actuar)
            {
                if (pieza.comprobarIzquierda())
                {
                    pieza.izquierda();
                }
                actuar = false;
            }
        }
    }


    public void bajar()
    {
        if (!pausado)
        {
            if (pieza.comprobarBajar())
            {
                pieza.bajar();
            }
            else
            {
                CancelInvoke("bajar");
                InvokeRepeating("bajar", 0.0f, modelo.getVelocidad());
            }
        }
    }

    class Posicion
    {
        private int x, y;
        public Posicion(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int getX()
        {
            return x;
        }

        public void setX(int x)
        {
            this.x = x;
        }

        public int getY()
        {
            return y;
        }

        public void setY(int y)
        {
            this.y = y;
        }

        public void bajar()
        {
            x++;
        }
        public void derecha()
        {
            y++;
        }
        public void izquierda()
        {
            y--;
        }

    }

    class Pieza
    {
        private const int NUM_POSICIONES = 4;
        private const int VACIO = 0;
        private const int PIEZA_ACTUAL = 2;
        private const int PIEZA_MUERTA = 1;
        private Posicion[] posiciones  = new Posicion[4];
        private Posicion [] posicionesNuevas = new Posicion[4];
        private int tipo;
        private int nextTipo;
        private int estado;
        private ModeloTetris modelo;

        public Pieza(int tipo, int nextTipo)
        {
            this.tipo = tipo;
            this.nextTipo = nextTipo;
            modelo = ModeloTetris.getModelo();
            posiciones = new Posicion[NUM_POSICIONES];
            posicionesNuevas = new Posicion[NUM_POSICIONES];
            inicializarPieza();
        }

        public Posicion[] getPosiciones()
        {
            return posiciones;
        }

        private void inicializarPieza()
        {
            estado = 0;
            if (tipo == 0)
            {
                posiciones[0] = new Posicion(0, 4);
                posiciones[1] = new Posicion(0, 5);
                posiciones[2] = new Posicion(1, 4);
                posiciones[3] = new Posicion(1, 5);
            }
            else if (tipo == 1)
            {
                posiciones[0] = new Posicion(0, 3);
                posiciones[1] = new Posicion(0, 4);
                posiciones[2] = new Posicion(0, 5);
                posiciones[3] = new Posicion(0, 6);
            }
            else if (tipo == 2)
            {
                posiciones[0] = new Posicion(0, 5);
                posiciones[1] = new Posicion(1, 4);
                posiciones[2] = new Posicion(1, 5);
                posiciones[3] = new Posicion(1, 6);
            }
            else if (tipo == 3)
            {
                posiciones[0] = new Posicion(0, 5);
                posiciones[1] = new Posicion(0, 6);
                posiciones[2] = new Posicion(1, 4);
                posiciones[3] = new Posicion(1, 5); ;
            }
            else if (tipo == 4)
            {
                posiciones[0] = new Posicion(0, 4);
                posiciones[1] = new Posicion(0, 5);
                posiciones[2] = new Posicion(1, 5);
                posiciones[3] = new Posicion(1, 6);
            }
            else if (tipo == 5)
            {
                posiciones[0] = new Posicion(0, 5);
                posiciones[1] = new Posicion(1, 5);
                posiciones[2] = new Posicion(2, 5);
                posiciones[3] = new Posicion(2, 6);
            }
            else if (tipo == 6)
            {
                posiciones[0] = new Posicion(0, 5);
                posiciones[1] = new Posicion(1, 5);
                posiciones[2] = new Posicion(2, 4);
                posiciones[3] = new Posicion(2, 5);
            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                posicionesNuevas[i] = new Posicion(posiciones[i].getX(), posiciones[i].getY());
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), PIEZA_ACTUAL);
            }
        }

        public bool comprobarBajar()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                if (modelo.getValorTetris((posiciones[i].getX() + 1), posiciones[i].getY()) == PIEZA_MUERTA)
                {
                    matar();
                    return false;
                }
            }
            return true;
        }

        public bool comprobarDerecha()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                if (modelo.getValorTetris((posiciones[i].getX()), posiciones[i].getY() + 1) == PIEZA_MUERTA)
                {
                    return false;
                }
            }
            return true;
        }

        public bool comprobarIzquierda()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                if (modelo.getValorTetris((posiciones[i].getX()), posiciones[i].getY() - 1) == PIEZA_MUERTA)
                {
                    //matar();
                    return false;
                }
            }
            return true;
        }

        public void izquierda()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                posicionesNuevas[i].izquierda();

            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), VACIO);
            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posicionesNuevas[i].getX(), posicionesNuevas[i].getY(), PIEZA_ACTUAL);
                posiciones[i].setX(posicionesNuevas[i].getX());
                posiciones[i].setY(posicionesNuevas[i].getY());
            }
        }

        public void derecha()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                posicionesNuevas[i].derecha();

            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), VACIO);
            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posicionesNuevas[i].getX(), posicionesNuevas[i].getY(), PIEZA_ACTUAL);
                posiciones[i].setX(posicionesNuevas[i].getX());
                posiciones[i].setY(posicionesNuevas[i].getY());
            }
        }

        public int getTipo()
        {
            return tipo;
        }

        public int getNextTipo()
        {
            return nextTipo;
        }

        public void setNextTipo(int tipo)
        {
            nextTipo = tipo;
        }

        public bool contenida(int x, int y)
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                if (posiciones[i].getX() == x && posiciones[i].getY() == y)
                {
                    return true;
                }
            }
            return false;
        }

        public bool comprobarGiro(int sentido)
        {
            if (tipo == 0)
            {

            }
            else if (tipo == 1)
            {
                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX());
                    posicionesNuevas[0].setY(posiciones[0].getY() + 2);

                    posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                    posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() + 3);
                    posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                }
                else if (estado == 1)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX());
                    posicionesNuevas[0].setY(posiciones[0].getY() - 2);

                    posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                    posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() - 3);
                    posicionesNuevas[3].setY(posiciones[3].getY() + 1);

                }
            }
            else if (tipo == 2)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 0);
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 0);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 0);
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 0);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 0);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 0);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 0);
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 0);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 0);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 0);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 0);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }
                }
                //aquie comprobar
                else if (sentido == -1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX());
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX());
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }

                }
            }
            else if (tipo == 3)
            {
                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                    posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                    posicionesNuevas[1].setX(posiciones[1].getX());
                    posicionesNuevas[1].setY(posiciones[1].getY() - 2);

                    posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                    posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                    posicionesNuevas[3].setX(posiciones[3].getX());
                    posicionesNuevas[3].setY(posiciones[3].getY());
                }
                else if (estado == 1)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                    posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                    posicionesNuevas[1].setX(posiciones[1].getX());
                    posicionesNuevas[1].setY(posiciones[1].getY() + 2);

                    posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                    posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                    posicionesNuevas[3].setX(posiciones[3].getX());
                    posicionesNuevas[3].setY(posiciones[3].getY());
                }
            }
            else if (tipo == 4)
            {
                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() + 2);
                    posicionesNuevas[0].setY(posiciones[0].getY());

                    posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                    posicionesNuevas[2].setX(posiciones[2].getX());
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                    posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                }
                else if (estado == 1)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() - 2);
                    posicionesNuevas[0].setY(posiciones[0].getY());

                    posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                    posicionesNuevas[2].setX(posiciones[2].getX());
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                    posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                }
            }
            else if (tipo == 5)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() - 2);
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() + 2);
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                }
                else if (sentido == -1)
                {

                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() + 2);
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() - 2);
                    }
                }
            }
            else if (tipo == 6)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() + 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() - 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                }
                else if (sentido == -1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() + 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() - 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    }
                }
            }
            if (piezaValida(posicionesNuevas))
            {
                return true;
            }
            else
            {
                for (int i = 0; i < posicionesNuevas.Length; i++)
                {
                    posicionesNuevas[i].setX(posiciones[i].getX());
                    posicionesNuevas[i].setY(posiciones[i].getY());
                }
                return false;
            }
        }

        public void girar(int sentido)
        {
            if (tipo == 0)
            {

            }
            else if (tipo == 1)
            {
                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX());
                    posicionesNuevas[0].setY(posiciones[0].getY() + 2);

                    posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                    posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() + 3);
                    posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    estado = 1;
                }
                else if (estado == 1)
                {

                    posicionesNuevas[0].setX(posiciones[0].getX());
                    posicionesNuevas[0].setY(posiciones[0].getY() - 2);

                    posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                    posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() - 3);
                    posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    estado = 0;
                }
            }
            else if (tipo == 2)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 0);
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 0);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 0);
                        estado += sentido;
                        if (estado < 0)
                        {
                            estado = 3;
                        }
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 0);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 0);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 0);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 0);
                        estado += sentido;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 0);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 0);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 0);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 0);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado += sentido;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                        posicionesNuevas[2].setX(posiciones[2].getX() + 0);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 0);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado += sentido;
                        if (estado > 3)
                        {
                            estado = 0;
                        }
                    }
                }
                //aqui
                else if (sentido == -1)
                {
                    //sentido = 1;
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado = 3;
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX());
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                        posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 0;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 1;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX());
                        posicionesNuevas[0].setY(posiciones[0].getY());

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado = 2;
                    }
                }
            }
            else if (tipo == 3)
            {

                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                    posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                    posicionesNuevas[1].setX(posiciones[1].getX());
                    posicionesNuevas[1].setY(posiciones[1].getY() - 2);

                    posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                    posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                    posicionesNuevas[3].setX(posiciones[3].getX());
                    posicionesNuevas[3].setY(posiciones[3].getY());
                    estado = 1;
                }
                else if (estado == 1)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                    posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                    posicionesNuevas[1].setX(posiciones[1].getX());
                    posicionesNuevas[1].setY(posiciones[1].getY() + 2);

                    posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                    posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                    posicionesNuevas[3].setX(posiciones[3].getX());
                    posicionesNuevas[3].setY(posiciones[3].getY());
                    estado = 0;
                }

            }
            else if (tipo == 4)
            {
                if (estado == 0)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() + 2);
                    posicionesNuevas[0].setY(posiciones[0].getY());

                    posicionesNuevas[1].setX(posiciones[1].getX() + 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() - 1);

                    posicionesNuevas[2].setX(posiciones[2].getX());
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                    posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                    estado = 1;
                }
                else if (estado == 1)
                {
                    posicionesNuevas[0].setX(posiciones[0].getX() - 2);
                    posicionesNuevas[0].setY(posiciones[0].getY());

                    posicionesNuevas[1].setX(posiciones[1].getX() - 1);
                    posicionesNuevas[1].setY(posiciones[1].getY() + 1);

                    posicionesNuevas[2].setX(posiciones[2].getX());
                    posicionesNuevas[2].setY(posiciones[2].getY());

                    posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                    posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                    estado = 0;
                }
            }
            else if (tipo == 5)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() - 2);
                        estado = 1;
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 2;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() + 2);
                        estado = 3;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 0;
                    }
                }
                else if (sentido == -1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 3;
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() + 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() + 2);
                        estado = 0;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 2);
                        posicionesNuevas[3].setY(posiciones[3].getY());
                        estado = 1;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 1);
                        posicionesNuevas[2].setY(posiciones[2].getY() - 1);

                        posicionesNuevas[3].setX(posiciones[3].getX());
                        posicionesNuevas[3].setY(posiciones[3].getY() - 2);
                        estado = 2;
                    }
                }
            }
            else if (tipo == 6)
            {
                if (sentido == 1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado = 1;
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() + 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado = 2;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado = 3;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() - 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado = 0;
                    }
                }
                else if (sentido == -1)
                {
                    if (estado == 0)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() + 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado = 3;
                    }
                    else if (estado == 1)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() - 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() + 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() + 1);
                        estado = 0;
                    }
                    else if (estado == 2)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() - 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX());
                        posicionesNuevas[2].setY(posiciones[2].getY() - 2);

                        posicionesNuevas[3].setX(posiciones[3].getX() + 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado = 1;
                    }
                    else if (estado == 3)
                    {
                        posicionesNuevas[0].setX(posiciones[0].getX() + 1);
                        posicionesNuevas[0].setY(posiciones[0].getY() + 1);

                        posicionesNuevas[1].setX(posiciones[1].getX());
                        posicionesNuevas[1].setY(posiciones[1].getY());

                        posicionesNuevas[2].setX(posiciones[2].getX() - 2);
                        posicionesNuevas[2].setY(posiciones[2].getY());

                        posicionesNuevas[3].setX(posiciones[3].getX() - 1);
                        posicionesNuevas[3].setY(posiciones[3].getY() - 1);
                        estado = 2;
                    }
                }
            }

            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), VACIO);
            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posicionesNuevas[i].getX(), posicionesNuevas[i].getY(), PIEZA_ACTUAL);
                posiciones[i].setX(posicionesNuevas[i].getX());
                posiciones[i].setY(posicionesNuevas[i].getY());
            }
        }

        public void bajar()
        {

            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                posicionesNuevas[i].bajar();

            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), VACIO);
            }
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posicionesNuevas[i].getX(), posicionesNuevas[i].getY(), PIEZA_ACTUAL);
                posiciones[i].setX(posicionesNuevas[i].getX());
                posiciones[i].setY(posicionesNuevas[i].getY());
            }

        }

        private bool piezaValida(Posicion[] posiciones)
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                if (posiciones[i].getX() < 0 || posiciones[i].getX() >= modelo.getFilas()
                        || posiciones[i].getY() < 0 || posiciones[i].getY() > modelo.getColumnas())
                {
                    return false;
                }
                else
                {
                    if (modelo.getValorTetris(posiciones[i].getX(), posiciones[i].getY()) == PIEZA_MUERTA)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void matar()
        {
            for (int i = 0; i < posiciones.GetLength(0); i++)
            {
                modelo.setValor(posiciones[i].getX(), posiciones[i].getY(), PIEZA_MUERTA);
            }
            modelo.comprobarFin();
            tipo = nextTipo;
            nextTipo = Random.Range(0, 7);
            //tipo = 1;
            inicializarPieza();
        }
    }

    class ModeloTetris
    {
        private const int VACIO = 0;
        private const int PIEZA_ACTUAL = 2;
        private const int PIEZA_MUERTA = 1;
        private const int FILAS = 21;
        private const int COLUMNAS = 12;
        private int lineas;
        private int puntuacion;
        private int[ , ] tetris = new int[FILAS, COLUMNAS];
        private float velocidad;
        private int maxPuntuacion;
        private static ModeloTetris modelo = null;

        private ModeloTetris()
        {
            inicializar();
        }

        private void inicializar()
        {
            velocidad = 0.8f;
            
            puntuacion = 0;
            lineas = 0;
            for (int i = 0; i < tetris.GetLength(0); i++)
            {
                for (int j = 0; j < tetris.GetLength(1); j++)
                {
                    if (j == 0 || j == COLUMNAS - 1 || i == FILAS - 1)
                    {
                        tetris[i, j] = PIEZA_MUERTA;
                    }
                    else
                    {
                        tetris[i, j] = VACIO;
                    }
                }
            }
        }

        public void setLineas(int l)
        {
            lineas = l;
        }

        public int getLineas()
        {
            return lineas;
        }

        public void setPuntuacion(int l)
        {
            puntuacion = l;
        }

        public int getPuntuacion()
        {
            return puntuacion;
        }

        public float getVelocidad()
        {
            return velocidad;
        }

        public bool comprobarFin()
        {
            for (int i = 1; i < tetris.GetLength(1) - 1; i++)
            {
                if (tetris[0, i] == PIEZA_MUERTA)
                {
                    inicializar();
                    return true;
                }
            }
            return false;
        }

        public static ModeloTetris getModelo()
        {
            if (modelo == null)
            {
                modelo = new ModeloTetris();
            }
            return modelo;
        }

        public void setValor(int x, int y, int valor)
        {
            tetris[x, y] = valor;
        }

        public int getValorTetris(int x, int y)
        {
            return tetris[x, y];
        }

        public int[, ] getTetris()
        {
            return tetris;
        }

        public int getFilas()
        {
            return FILAS;
        }

        public int getColumnas()
        {
            return COLUMNAS;
        }

        public void limpirar(Pieza pieza)
        {
            for (int i = 0; i < tetris.GetLength(0); i++)
            {
                for (int j = 0; j < tetris.GetLength(1); j++)
                {
                    if (tetris[i, j] == PIEZA_ACTUAL && !pieza.contenida(i, j))
                    {
                        tetris[i, j] = VACIO;
                    }
                }
            }
        }

        public int getMaxPuntuacion()
        {
            return PlayerPrefs.GetInt("Record", 0);
        }

        public void setMaxPuntuacion(int record)
        {
            if(record > this.maxPuntuacion)
            {
                maxPuntuacion = record;
                PlayerPrefs.SetInt("Record", maxPuntuacion);
            }
        }

        public void comprobarLinea()
        {
            int puntosaux = 0;
            int[] aux = new int[tetris.GetLength(1) - 2];
            for (int i = 0; i < tetris.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < tetris.GetLength(1) - 1; j++)
                {
                    aux[j - 1] = tetris[i, j];
                }
                if (comprobarAux(aux))
                {
                    puntosaux++;
                    lineas++;
                    borrarLinea(i);
                }
            }
            puntuacion += calcularPuntuacion(puntosaux, velocidad);

            setMaxPuntuacion(puntuacion);
        }
        public int calcularPuntuacion(int lineas, float velocidad)
        {
            float aux = 0;

            if (lineas == 1)
            {
                aux = (lineas / velocidad) * 10;
            }
            else if (lineas == 2)
            {
                aux = (2.5f / velocidad) * 10;
            }
            else if (lineas == 3)
            {
                aux = (3.5f / velocidad) * 10;
            }
            else if (lineas == 4)
            {
                aux = (5 / velocidad) * 10;
            }
            return (int)System.Math.Round(aux);
        }
        private void borrarLinea(int linea)
        {
            if (lineas % 10 == 0)
            {
                if (velocidad - 0.10f >= 0.10f)
                {
                    velocidad -= 0.10f;
                }
                else
                {
                    if (velocidad - 0.010f > 0f)
                    {
                        velocidad -= 0.010f;
                    }
                }
                velocidad = (float) System.Math.Round(velocidad, 2);
            }
            
            for (int i = 0; i < tetris.GetLength(1); i++)
            {
                tetris[linea, i] = VACIO;
            }
            for (int i = linea; i >= 0; i--)
            {
                for (int j = 0; j < tetris.GetLength(1); j++)
                {
                    if (i - 1 >= 0)
                    {
                        tetris[i, j] = tetris[i - 1, j];
                    }
                    else
                    {
                        for (int k = 0; k < tetris.GetLength(1); k++)
                        {
                            if (k == 0 || k == tetris.GetLength(1) - 1)
                            {
                                tetris[0, k] = PIEZA_MUERTA;
                            }
                            else
                            {
                                tetris[0, k] = VACIO;
                            }
                        }
                    }
                }
            }
        }

        private bool comprobarAux(int[] aux)
        {
            for (int i = 0; i < aux.GetLength(0); i++)
            {
                if (aux[i] != PIEZA_MUERTA)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
