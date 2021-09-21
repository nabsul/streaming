package main

import (
	"bufio"
	"fmt"
	"log"
	"math/rand"
	"os"
	"time"
)

func incrementDate(d time.Time) time.Time {
	return d.Add(time.Second * time.Duration(rand.Int() % 10))
}

func main() {
	startDate := time.Date(2021, 9,1, 0, 0, 0, 0, time.UTC)
	endDate := time.Date(2021,10,1, 0, 0, 0, 0, time.UTC)

	fmt.Println("Opening file")
	file, err := os.Create("./data.json")
	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Writing data...")
	writer := bufio.NewWriter(file)
	date := incrementDate(startDate)
	for date.Before(endDate) {
		_, err := writer.WriteString(date.String() + "\n")
		if err != nil {
			log.Fatalf("Got error while writing to a file. Err: %s", err.Error())
		}
		date = incrementDate(date)
	}

	writer.Flush()
	fmt.Println("Done.")
}
