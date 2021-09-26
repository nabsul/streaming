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

	write(writer, "[")

	prefix := "\n\t"
	date := incrementDate(startDate)
	for date.Before(endDate) {
		write(writer, prefix + date.String() + "\n")
		date = incrementDate(date)
		prefix = ",\n\t"
	}

	write(writer, "\n]\n")
	writer.Flush()
	fmt.Println("Done.")
}

func write(writer *bufio.Writer, str string) {
	_, err := writer.WriteString(str)
	if err != nil {
		log.Fatalf("Failed to write to file: %s", err.Error())
	}
}
