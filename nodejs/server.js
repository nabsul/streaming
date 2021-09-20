const hostname = '127.0.0.1';
const port = 3000;

const { createReadStream } = require('fs')
const { pipeline } = require('stream/promises')
const { createServer } = require('http')
const { createCsvTransform, createJsonTransform, createSummaryTransform, createRawDataTransform } = require('./transforms')

const handleCsvRequest = async (res) => {
    res.statusCode = 200
    res.setHeader('Content-Type', 'text/plain')
    const file = createReadStream('data.json')
    const parse = createRawDataTransform()
    const transform = createCsvTransform()
    await pipeline(file, parse, transform, res)
}

const handleSummaryRequest = async (res) => {
    res.statusCode = 200
    res.setHeader('Content-Type', 'application/json')
    const file = createReadStream('data.json')
    const parse = createRawDataTransform()
    const transform = createSummaryTransform()
    const json = createJsonTransform()
    await pipeline(file, parse, transform, json, res)
}

const map = {
    '/csv': handleCsvRequest,
    '/summary': handleSummaryRequest,
}

const handleRequest = async (req, res) => {
    try {
        const handler = map[req.url]
        if (handler) {
            return await handler(res)
        }

        res.statusCode = 400
        res.setHeader('Content-Type', 'application/json')
        await res.end(JSON.stringify({success: false, message: 'not found'}))
    } catch (error) {
        console.log(`error: ${error}`)
    }
}

createServer(handleRequest).listen(port, hostname, () => {
    console.log(`Server running at http://${hostname}:${port}/`);
});
