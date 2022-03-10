use std::fs::File;
use actix_web::{get, web, App, HttpServer, Responder};
use serde::{Serialize, Deserialize};

#[get("/csv")]
async fn csv() -> impl Responder {
    let it = get_entries();

    while it.next() {
    }

    format!("")
}

#[get("/summary")]
async fn summary() -> impl Responder {
    format!("Hello ! id:")
}

#[actix_web::main]
pub async fn serve() -> std::io::Result<()> {
    HttpServer::new(|| App::new().service(csv).service(summary))
        .bind("127.0.0.1:8080")?
        .run()
        .await
}

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

fn get_entries() -> EntryStream {
    let f = File::open("data.json").unwrap();
    return EntryStream { file: f };
}
