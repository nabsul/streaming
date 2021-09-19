const hostname = '127.0.0.1';
const port = 3000;

const { createReadStream } = require('fs')
const { pipeline } = require('stream/promises')
const { createServer } = require('http')
const { Transform } = require('stream')

const createJsonTransform = () => {
    let first = true
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            let prefix = first ? '[\n\t' : ',\n\t'
            first = false
            this.push(prefix + JSON.stringify(obj))
            cb()
        },
        final(cb) {
            this.push('\n]')
            cb()
        }
    })
}

const createSummaryTransform = () => {
    let currentDay = -1
    let totalScores = new Map()
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            const day = obj.time.substr(0, obj.time.indexOf('T'))

            // flush outputs if necessary
            if (day !== currentDay) {
                currentDay = day

                if (totalScores.size > 0) {
                    let winner = null
                    let maxScore = -1
                    for (const entry of totalScores.entries()) {
                        const [user, score] = entry
                        if (score < maxScore) continue
                        maxScore = score
                        winner = user
                    }

                    this.push({day: currentDay, winner: winner, totalScore: maxScore})
                    totalScores = new Map()
                }
            }

            const score = (totalScores.get(obj.userId) || 0) + obj.points
            totalScores.set(obj.userId, score)
            cb()
        }
    })
}

const createCsvTransform = () => {
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            cb(null, `${obj.time},${obj.userId},${obj.points}\n`)
        }
    })
}

const createRawDataTransform = () => {
    let buffer = ''
    return new Transform({
        objectMode: true,
        transform(chunk, enc, cb) {
            buffer += chunk.toString()
            while (true) {
                const openPos = buffer.indexOf('{')
                if (openPos < 0) break

                const closePos = buffer.indexOf('}')
                if (closePos < 0) break

                const substr = buffer.substr(openPos, closePos - openPos + 1)
                this.push(JSON.parse(substr))
                buffer = buffer.substr(closePos + 1)
            }
            cb()
        }
    })
}
const handleRequest = async (req, res) => {
    console.log(`recieving request: ${req.url}`)
    if (req.url === '/csv') {
        res.statusCode = 200
        res.setHeader('Content-Type', 'text/plain')
        const file = createReadStream('data.json')
        const parse = createRawDataTransform()
        const transform = createCsvTransform()
        await pipeline(file, parse, transform, res)
        return
    }

    if (req.url === '/summary') {
        res.statusCode = 200
        res.setHeader('Content-Type', 'application/json')
        const file = createReadStream('data.json')
        const parse = createRawDataTransform()
        const transform = createSummaryTransform()
        const json = createJsonTransform()
        await pipeline(file, parse, transform, json, res)
        return
    }

    res.statusCode = 400
    res.setHeader('Content-Type', 'application/json')
    await res.end(JSON.stringify({success: false, message: 'not found'}))
}

createServer(handleRequest).listen(port, hostname, () => {
    console.log(`Server running at http://${hostname}:${port}/`);
});
