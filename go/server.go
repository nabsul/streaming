package main

import (
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
)

func main() {
	http.HandleFunc("/summary", summaryHandler)
	http.HandleFunc("/csv", csvHandler)
	log.Fatal(http.ListenAndServe(":8000", nil))
}

type Entry struct {
	Time string `json:time`
	UserId string `json:userId`
	Points int `json:points`
}

func csvHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Println("Received CSV request")
	file, err := os.Open("./data.json")
	checkErr(err)

	dec := json.NewDecoder(file)
	dec.Token()
	for dec.More() {
		var e Entry
		err = dec.Decode(&e)
		checkErr(err)

		_, err = fmt.Fprintf(w, "%s,%s,%d\n", e.Time, e.UserId, e.Points)
		checkErr(err)
	}
}

func summaryHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Println("Received summary request")
	io.WriteString(w, "Hello world")
}

func checkErr(err error) {
	if err != nil {
		log.Fatal(err)
	}
}
