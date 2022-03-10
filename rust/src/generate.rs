use chrono;
use rand;
use serde::{Serialize, Deserialize};
use serde_json;
use std::fs::File;
use std::io::Write;

#[derive(Serialize, Deserialize, Debug)]
#[allow(non_snake_case)]
struct Entry {
  time: String,
  userId: String,
  points: u32,
}

struct EntryStream {
    file: File,
}

impl Iterator for EntryStream {
    type Item = Entry;

    fn next(&mut self) -> Option<Entry> {
        None
    }
}

pub fn get_entries() -> EntryStream {
    let f = File::open("data.json").unwrap();
    return EntryStream { file: f };
}

pub fn generate() {
    println!("Getting ready to generate file...");
    let mut output = File::create("data.json").unwrap();
    write!(output, "[").unwrap();

    let mut prefix = "\n\t";

    let start = chrono::NaiveDate::from_ymd(2021, 9, 1).and_hms(0, 0, 0);
    let end = chrono::NaiveDate::from_ymd(2021, 10, 1).and_hms(0, 0, 0);
    let mut date = increment(start);

    println!("Writing data...");
    while date < end {
        let entry = Entry {
            time: date.to_string(),
            userId: format!("user{}", rand::random::<u32>() % 100),
            points: rand::random::<u32>() % 100,
        };

        write!(output, "{}{}", prefix, serde_json::to_string(&entry).unwrap()).unwrap();
        prefix = ",\n\t";
        date = increment(date);
    }

    write!(output, "\n]\n").unwrap();

    println!("done.")
}

fn increment(date: chrono::NaiveDateTime) -> chrono::NaiveDateTime {
    let rnd = rand::random::<u32>() % 10;
    let sec = chrono::Duration::seconds(rnd as i64);
    return date + sec;
}
